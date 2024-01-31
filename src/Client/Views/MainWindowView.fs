module MainWindowView

open Feliz
open Feliz.Bulma
open Messages
open Shared

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
                                Bulma.tab  [
                                    prop.style [style.width (length.px 20)]
                                ]
                                
                                MainComponents.FooterTabs.MainMetadata (model, dispatch)
                                for index in 0 .. (model.SpreadsheetModel.Tables.TableCount-1) do
                                    MainComponents.FooterTabs.Main (index, model.SpreadsheetModel.Tables, model, dispatch)
                                match model.SpreadsheetModel.ArcFile with
                                | Some (ArcFiles.Template _) | Some (ArcFiles.Investigation _) ->
                                    Html.none
                                | _ ->
                                    MainComponents.FooterTabs.MainPlus dispatch
                                MainComponents.FooterTabs.ToggleSidebar(model, dispatch)
                            ]
                        ]
                    ]
                ]
            ]
        ]
    ]

open Shared

[<ReactComponent>]
let Main (model: Messages.Model, dispatch) =
    let state = model.SpreadsheetModel
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
                    | Some (ArcFiles.Assay _) 
                    | Some (ArcFiles.Study _)
                    | Some (ArcFiles.Investigation _) 
                    | Some (ArcFiles.Template _) ->
                        XlsxFileView.Main (model , dispatch)
                    if state.Tables.TableCount > 0 && state.ActiveTable.ColumnCount > 0 && state.ActiveView <> Spreadsheet.ActiveView.Metadata then
                        MainComponents.AddRows.Main dispatch
                ]
            ]
            if state.ArcFile.IsSome then 
                spreadsheetSelectionFooter model dispatch
        ]
    ]