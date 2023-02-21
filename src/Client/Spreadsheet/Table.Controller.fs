module Spreadsheet.Table.Controller

open System.Collections.Generic
open Shared.TermTypes
open Shared.OfficeInteropTypes
open Spreadsheet
open Parser
open Types
open Helper

/// <summary>This function is used to save the active table to the tables map. is only executed if tables map is not empty.</summary>
let saveActiveTable (state: Spreadsheet.Model) : Spreadsheet.Model =
    if Map.isEmpty state.Tables then
        state
    else
        let parsed_activeTable = state.ActiveTable |> SwateBuildingBlock.ofTableMap
        let nextTable =
            let t = state.Tables.[state.ActiveTableIndex]
            {t with BuildingBlocks = parsed_activeTable}
        let nextTables = state.Tables.Change(state.ActiveTableIndex, fun _ -> Some nextTable)
        {state with Tables = nextTables}

let updateTableOrder (prevIndex:int, newIndex:int) (m:Map<'a,int>) =
    m
    |> Map.toSeq
    |> Seq.map (fun (id, order) ->
        // the element to switch
        if order = prevIndex then 
            id, newIndex
        // keep value if smaller then newIndex
        elif order < newIndex then
            id, order
        elif order >= newIndex then
            id, order + 1
        else
            failwith "this should never happen"
    )
    |> Seq.sortBy snd
    // rebase order, this prevents ordering above "+" symbol in footer with System.Int32.MaxValue
    |> Seq.mapi (fun i (id, _) -> id, i)
    |> Map.ofSeq

let resetTableState() : Spreadsheet.Model =
    Spreadsheet.LocalStorage.resetAll()
    Spreadsheet.Model.init()

let removeTable (removeIndex: int) (state: Spreadsheet.Model) : Spreadsheet.Model =
    let nextTables = state.Tables.Remove(removeIndex)
    // If the only existing table was removed init model from beginning
    if nextTables = Map.empty then
        Spreadsheet.Model.init()
    else
        // if active table is removed get the next closest table and set it active
        if state.ActiveTableIndex = removeIndex then
            let nextTable_Index =
                let neighbors = findNeighborTables removeIndex nextTables
                match neighbors with
                | Some (i, _), _ -> i
                | None, Some (i, _) -> i
                // This is a fallback option
                | _ -> nextTables.Keys |> Seq.head
            let nextTable = state.Tables.[nextTable_Index].BuildingBlocks |> SwateBuildingBlock.toTableMap
            { state with
                ActiveTableIndex = nextTable_Index
                Tables = nextTables
                ActiveTable = nextTable }
        // Tables still exist and an inactive one was removed. Just remove it.
        else
            { state with Tables = nextTables }

///<summary>Add `n` rows to active table.</summary>
let addRows (n: int) (state: Spreadsheet.Model) : Spreadsheet.Model =
    let keys = state.ActiveTable.Keys
    let maxRow = keys |> Seq.maxBy snd |> snd
    let maxCol = keys |> Seq.maxBy fst |> fst
    /// create new keys to add to active table
    let newKeys = [
        // iterate over all columns
        for c in 0 .. maxCol do
            // then create for EACH COLUMN and EACH ROW ABOVE maxRow UNTIL maxRow + number of new rows
            for r in (maxRow + 1) .. (maxRow + n) do
                yield c, r
    ]
    /// This MUST be 0, so no overlap between existing keys and new keys exists.
    /// This is important as Map.add would replace previous values on that key.
    let checkNewKeys =
        let keySet = keys |> Set.ofSeq
        let newKeysSet = newKeys |> Set.ofList
        Set.intersect keySet newKeysSet
        |> Set.count
    if checkNewKeys <> 0 then failwith "Error in `addRows` function. Unable to add new rows without replacing existing values. Please contact us with a bug report."
    let nextActiveTable =
        let prev = state.ActiveTable |> Map.toList
        let next = newKeys |> List.map (fun x -> x, SwateCell.create(TermMinimal.empty))
        prev@next
        |> Map.ofList
    let nextState = {state with ActiveTable = nextActiveTable}
    nextState

let deleteRow (index: int) (state: Spreadsheet.Model) : Spreadsheet.Model =
    let nextTable = 
        state.ActiveTable
        |> Map.toArray
        |> Array.filter (fun ((_,r),_) -> r <> index)
        |> Array.map (fun ((c,r),cvalue) ->
            let updateIndex = if r > index then r-1 else r
            (c,updateIndex), cvalue
        )
        |> Map.ofArray
    {state with ActiveTable = nextTable}

let deleteColumn (index: int) (state: Spreadsheet.Model) : Spreadsheet.Model =
    let nextTable = 
        state.ActiveTable
        |> Map.toArray
        |> Array.filter (fun ((c,_),_) -> c <> index)
        |> Array.map (fun ((c,r),cvalue) ->
            let updateIndex = if c > index then c-1 else c
            (updateIndex,r), cvalue
        )
        |> Map.ofArray
    {state with ActiveTable = nextTable}

let mutable clipboardCell: SwateCell option = None

let copyCell (index: int*int) (state: Spreadsheet.Model) : Spreadsheet.Model =
    let cell = state.ActiveTable.[index]
    clipboardCell <- Some cell
    state

let copySelectedCell (state: Spreadsheet.Model) : Spreadsheet.Model =
    /// Array.head is used until multiple cells are supported, should this ever be intended
    let index = state.SelectedCells |> Set.toArray |> Array.head
    copyCell index state

let cutCell (index: int*int) (state: Spreadsheet.Model) : Spreadsheet.Model =
    let cell = state.ActiveTable.[index]
    // Remove selected cell value
    let emptyCell = if cell.isBody then SwateCell.emptyBody elif cell.isHeader then SwateCell.emptyHeader else failwithf "Could not read cell type for index: %A" index
    let nextActiveTable = state.ActiveTable.Add(index, emptyCell)
    let nextState = {state with ActiveTable = nextActiveTable}
    clipboardCell <- Some cell
    nextState

let cutSelectedCell (state: Spreadsheet.Model) : Spreadsheet.Model =
    /// Array.head is used until multiple cells are supported, should this ever be intended
    let index = state.SelectedCells |> Set.toArray |> Array.head
    cutCell index state

let insertCell (index: int*int) (state: Spreadsheet.Model) : Spreadsheet.Model =
    let selectedCell = state.ActiveTable.[index]
    let table = state.ActiveTable
    match clipboardCell with
    // Don't update if no cell in saved
    | None -> state
    | Some (IsBody cell) ->          
        // Don't update if saved cell is body and next cell is not body
        match selectedCell with
        | IsBody _ ->
            let nextTable = table.Add(index, IsBody cell)
            {state with ActiveTable = nextTable}
        | _ -> state
    | Some (IsHeader cell) ->
        // Don't update if saved cell is header and next cell is not header
        match selectedCell with
        | IsHeader _ ->
            let nextTable = table.Add(index, IsHeader cell)
            {state with ActiveTable = nextTable}
        | _ -> state

let insertSelectedCell (state: Spreadsheet.Model) : Spreadsheet.Model =
    let index = state.SelectedCells |> Set.toArray |> Array.head
    insertCell index state