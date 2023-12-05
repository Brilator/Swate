module MainWindowView

open Feliz
open Feliz.Bulma
open Messages

let private spreadsheetSelectionFooter (model: Messages.Model) dispatch =
    Html.div [
        prop.style [
            style.position.sticky;
            style.bottom 0
        ]
        prop.children [
            Html.div [
                prop.children [
                    Bulma.tabs [
                        prop.style [style.overflowY.visible]
                        Bulma.tabs.isBoxed
                        prop.children [
                            Html.ul [
                                yield Bulma.tab  [
                                    prop.style [style.width (length.px 20)]
                                ]
                                for index in 0 .. (model.SpreadsheetModel.Tables.TableCount-1) do
                                    yield
                                        MainComponents.FooterTabs.Main {| index = index; tables = model.SpreadsheetModel.Tables; model = model; dispatch = dispatch |}
                                yield
                                    MainComponents.FooterTabs.MainPlus {| dispatch = dispatch |}
                            ]
                        ]
                    ]
                ]
            ]
        ]
    ]

open Shared

[<ReactComponent>]
let Main (model: Messages.Model) dispatch =
    let state = model.SpreadsheetModel
    let init_RowsToAdd = 1
    let state_rows, setState_rows = React.useState(init_RowsToAdd)
    Html.div [
        prop.id "MainWindow"
        prop.style [
            style.display.flex
            style.flexDirection.column
            style.width (length.percent 100)
            style.height (length.percent 100)
        ]
        prop.children [
            MainComponents.Navbar.Main model dispatch
            Html.div [
                prop.id "TableContainer"
                prop.style [
                    style.width.inheritFromParent
                    style.height.inheritFromParent
                    style.overflowX.auto
                    style.display.flex
                    style.flexDirection.column
                ]
                prop.children [
                    //
                    match state.ArcFile with
                    | None ->
                        MainComponents.NoTablesElement.Main {|dispatch = dispatch|}
                    | Some (ArcFiles.Assay a) ->
                        MainComponents.SpreadsheetView.Main model dispatch
                    | Some (ArcFiles.Study (s,a)) ->
                        MainComponents.SpreadsheetView.Main model dispatch
                    | Some (ArcFiles.Investigation i) ->
                        Html.div "Investigation"
                    if state.Tables.TableCount > 0 && state.ActiveTable.ColumnCount > 0 then
                        MainComponents.AddRows.Main init_RowsToAdd state_rows setState_rows dispatch
                ]
            ]
            match state.Tables.TableCount = 0 with | true -> Html.none | false -> spreadsheetSelectionFooter model dispatch
        ]
    ]