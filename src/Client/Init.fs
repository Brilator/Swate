module Init

open Elmish.UrlParser
open Elmish
open LocalHistory
open Model
open Messages
open Update
open Thoth.Elmish

let initializeModel (pageOpt: Routing.Route option) =
    let dt = LocalStorage.Darkmode.DataTheme.GET()
    LocalStorage.Darkmode.DataTheme.SET dt
    {
        DebouncerState              = Debouncer                 .create ()
        PageState                   = PageState                 .init pageOpt
        PersistentStorageState      = PersistentStorageState    .init ()
        DevState                    = DevState                  .init ()
        TermSearchState             = TermSearch.Model          .init ()
        AdvancedSearchState         = AdvancedSearch.Model      .init ()
        ExcelState                  = OfficeInterop.Model       .init ()
        ApiState                    = ApiState                  .init ()
        FilePickerState             = FilePicker.Model          .init ()
        AddBuildingBlockState       = BuildingBlock.Model       .init ()
        ValidationState             = Validation.Model          .init ()
        ProtocolState               = Protocol.Model            .init ()
        BuildingBlockDetailsState   = BuildingBlockDetailsState .init ()
        SettingsXmlState            = SettingsXml.Model         .init ()
        JsonExporterModel           = JsonExporter.State.Model  .init ()
        TemplateMetadataModel       = TemplateMetadata.Model    .init ()
        DagModel                    = Dag.Model                 .init ()
        CytoscapeModel              = Cytoscape.Model           .init ()
        SpreadsheetModel            = Spreadsheet.Model         .fromLocalStorage()
        History                     = LocalHistory.Model        .init().UpdateFromSessionStorage()
    }

// defines the initial state and initial command (= side-effect) of the application
let init (pageOpt: Routing.Route option) : Model * Cmd<Msg> =
    let route = (parseHash Routing.Routing.route) Browser.Dom.document.location
    let initialModel = initializeModel (pageOpt)
    // The initial command from urlUpdate is not needed yet. As we use a reduced variant of subModels with no own Msg system.
    let model, _ = urlUpdate route initialModel
    let cmd = Cmd.ofMsg <| InterfaceMsg SpreadsheetInterface.Initialize 
    model, cmd