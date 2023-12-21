﻿module XlsxFileView

open Feliz
open Feliz.Bulma
open Messages
open Spreadsheet
open Shared

[<ReactComponentAttribute>]
let Main(x: {| model: Messages.Model; dispatch: Messages.Msg -> unit |}) = 
    let model, dispatch = x.model, x.dispatch
    match model.SpreadsheetModel.ActiveView with
    | ActiveView.Table _ ->
        MainComponents.SpreadsheetView.Main model dispatch
    | ActiveView.Metadata ->
        match model.SpreadsheetModel.ArcFile with
        | Some (ArcFiles.Assay a) ->
            MainComponents.Metadata.Assay.Main(a, model, dispatch)
        | Some (ArcFiles.Study (s,aArr)) ->
            MainComponents.Metadata.Study.Main(s, aArr, model, dispatch)
        | Some (ArcFiles.Investigation inv) ->
            MainComponents.Metadata.Investigation.Main(inv, model, dispatch)
        | Some (ArcFiles.Template _) ->
            MainComponents.Metadata.Template.Main()
        | None ->
            Html.none