[<AutoOpen>]
module Update.Update

open Elmish
open Thoth.Elmish

open Shared
open TermTypes
open OfficeInteropTypes
open Routing
open Model
open Messages

let urlUpdate (route: Route option) (currentModel:Model) : Model * Cmd<Messages.Msg> =
    match route with
    | Some (Route.Home queryIntegerOption) ->
        let swatehost = Swatehost.ofQueryParam queryIntegerOption
        let nextModel = {
            currentModel with 
                Messages.Model.PageState.CurrentPage = Route.BuildingBlock
                Messages.Model.PageState.IsExpert = false
                Messages.Model.PersistentStorageState.Host = Some swatehost
        }
        nextModel,Cmd.none
    | Some page ->
        let nextModel = {
            currentModel with 
                Messages.Model.PageState.CurrentPage = page
                Messages.Model.PageState.IsExpert = page.isExpert
        }
        nextModel,Cmd.none
    | None ->
        let nextModel = {
            currentModel with 
                Messages.Model.PageState.CurrentPage = Route.BuildingBlock
                Messages.Model.PageState.IsExpert = false
        }
        nextModel,Cmd.none

module AdvancedSearch =

    let update (msg: AdvancedSearch.Msg) (model:Messages.Model) : Messages.Model * Cmd<Messages.Msg> =
        match msg with
        | AdvancedSearch.GetSearchResults content -> 
            let cmd =
                Cmd.OfAsync.either 
                    Api.api.getTermsForAdvancedSearch
                    content.config
                    (fun terms -> Run (fun _ -> content.responseSetter terms))
                    (curry GenericError Cmd.none >> DevMsg)
                    
            model, cmd

module Dev = 

    let update (devMsg: DevMsg) (currentState:DevState) : DevState * Cmd<Messages.Msg> =
        match devMsg with
        | GenericLog (nextCmd,(level,logText)) ->
            let nextState = {
                currentState with
                    Log = (LogItem.ofStringNow level logText)::currentState.Log
            }
            nextState, nextCmd

        | GenericInteropLogs (nextCmd,logs) ->
            let parsedLogs = logs |> List.map LogItem.ofInteropLogginMsg
            let parsedDisplayLogs = parsedLogs |> List.filter (fun x -> match x with | Error _ | Warning _ -> true; | _ -> false)
            let nextState = {
                Log = parsedLogs@currentState.Log
                DisplayLogList = parsedDisplayLogs@currentState.DisplayLogList
            }
            let batch = Cmd.batch [
                let modalName = "GenericInteropLogs"
                if List.isEmpty parsedDisplayLogs |> not then Cmd.ofEffect(fun dispatch -> Modals.Controller.renderModal(modalName, Modals.InteropLoggingModal.interopLoggingModal(nextState, dispatch)))
                nextCmd
            ]
            nextState, batch

        | GenericError (nextCmd, e) ->
            let nextState = {
                currentState with
                    Log = LogItem.Error(System.DateTime.Now,e.GetPropagatedError())::currentState.Log
                }
            let batch = Cmd.batch [
                let modalName = "GenericError"
                Cmd.ofEffect(fun _ -> Modals.Controller.renderModal(modalName, Modals.ErrorModal.errorModal(e)))
                nextCmd
            ]
            nextState, batch

        | UpdateDisplayLogList newList ->
            let nextState = {
                currentState with
                    DisplayLogList = newList
            }
            nextState, Cmd.none

        | LogTableMetadata ->
            let cmd =
                Cmd.OfPromise.either
                    OfficeInterop.Core.getTableMetaData
                    ()
                    (curry GenericLog Cmd.none >> DevMsg)
                    (curry GenericError Cmd.none >> DevMsg)
            currentState, cmd

let handleApiRequestMsg (reqMsg: ApiRequestMsg) (currentState: ApiState) : ApiState * Cmd<Messages.Msg> =

    let handleTermSuggestionRequest (apiFunctionname:string) (responseHandler: Term [] -> ApiMsg) queryString =
        let currentCall = {
            FunctionName = apiFunctionname
            Status = Pending
        }

        let nextState = {
            currentState with
                currentCall = currentCall
        }
        let nextCmd = 
            Cmd.OfAsync.either
                Api.api.getTermSuggestions
                {|n= 5; query = queryString; ontology = None|}
                (responseHandler >> Api)
                (ApiError >> Api)

        nextState,nextCmd

    let handleUnitTermSuggestionRequest (apiFunctionname:string) (responseHandler: (Term []) -> ApiMsg) queryString =
        let currentCall = {
            FunctionName = apiFunctionname
            Status = Pending
        }

        let nextState = {
            currentState with
                currentCall = currentCall
        }
        let nextCmd = 
            Cmd.OfAsync.either
                Api.api.getUnitTermSuggestions
                {|n= 5; query = queryString|}
                (responseHandler >> Api)
                (ApiError >> Api)

        nextState,nextCmd

    let handleTermSuggestionByParentTermRequest (apiFunctionname:string) (responseHandler: Term [] -> ApiMsg) queryString (parent:TermMinimal) =
        let currentCall = {
            FunctionName = apiFunctionname
            Status = Pending
        }

        let nextState = {
            currentState with
                currentCall = currentCall
        }
        let nextCmd = 
            Cmd.OfAsync.either
                Api.api.getTermSuggestionsByParentTerm
                {|n= 5; query = queryString; parent_term = parent|}
                (responseHandler >> Api)
                (ApiError >> Api)

        nextState,nextCmd

    match reqMsg with

    | GetNewUnitTermSuggestions (queryString) ->
        handleUnitTermSuggestionRequest
            "getUnitTermSuggestions"
            (UnitTermSuggestionResponse >> Response)
            queryString

    | FetchAllOntologies ->
        let currentCall = {
                FunctionName = "getAllOntologies"
                Status = Pending
        }

        let nextState = {
            currentState with
                currentCall = currentCall
        }

        nextState,
        Cmd.OfAsync.either
            Api.api.getAllOntologies
            ()
            (FetchAllOntologiesResponse >> Response >> Api)
            (ApiError >> Api)

    | SearchForInsertTermsRequest (tableTerms) ->
        let currentCall = {
            FunctionName = "getTermsByNames"
            Status = Pending
        }
        let nextState = {
            currentState with
                currentCall = currentCall
        }
        let cmd =
            Cmd.OfAsync.either
                Api.api.getTermsByNames
                tableTerms
                (SearchForInsertTermsResponse >> Response >> Api)
                (fun e ->
                    Msg.Batch [
                        OfficeInterop.UpdateFillHiddenColsState OfficeInterop.FillHiddenColsState.Inactive |> OfficeInteropMsg
                        ApiError e |> Api
                    ] )
        let stateCmd = OfficeInterop.UpdateFillHiddenColsState OfficeInterop.FillHiddenColsState.ServerSearchDatabase |> OfficeInteropMsg |> Cmd.ofMsg
        let cmds = Cmd.batch [cmd; stateCmd]
        nextState, cmds
    //
    | GetAppVersion ->
        let currentCall = {
            FunctionName = "getAppVersion"
            Status = Pending
        }

        let nextState = {
            currentState with
                currentCall = currentCall
        }

        let cmd =
            Cmd.OfAsync.either
                Api.serviceApi.getAppVersion
                ()
                (GetAppVersionResponse >> Response >> Api)
                (ApiError >> Api)
            
        nextState, cmd
        

let handleApiResponseMsg (resMsg: ApiResponseMsg) (currentState: ApiState) : ApiState * Cmd<Messages.Msg> =

    let handleTermSuggestionResponse (responseHandler: Term [] -> Msg) (suggestions: Term[]) =
        let finishedCall = {
            currentState.currentCall with
                Status = Successfull
        }

        let nextState = {
            currentCall = ApiState.noCall
            callHistory = finishedCall::currentState.callHistory
        }

        let cmds = Cmd.batch [
            ("Debug",sprintf "[ApiSuccess]: Call %s successfull." finishedCall.FunctionName) |> ApiSuccess |> Api |> Cmd.ofMsg
            suggestions |> responseHandler |> Cmd.ofMsg
        ]

        nextState, cmds

    let handleUnitTermSuggestionResponse (responseHandler: Term [] -> Msg) (suggestions: Term[]) =
        let finishedCall = {
            currentState.currentCall with
                Status = Successfull
        }

        let nextState = {
            currentCall = ApiState.noCall
            callHistory = finishedCall::currentState.callHistory
        }

        let cmds = Cmd.batch [
            ("Debug",sprintf "[ApiSuccess]: Call %s successfull." finishedCall.FunctionName) |> ApiSuccess |> Api |> Cmd.ofMsg
            (suggestions) |> responseHandler |> Cmd.ofMsg
        ]

        nextState, cmds

    match resMsg with
    | UnitTermSuggestionResponse (suggestions) ->

        handleUnitTermSuggestionResponse
            (BuildingBlock.Msg.NewUnitTermSuggestions >> BuildingBlockMsg)
            suggestions            

    | FetchAllOntologiesResponse onts ->
        let finishedCall = {
            currentState.currentCall with
                Status = Successfull
        }

        let nextState = {
            currentCall = ApiState.noCall
            callHistory = finishedCall::currentState.callHistory
        }

        let cmds = Cmd.batch [
            ("Debug",sprintf "[ApiSuccess]: Call %s successfull." finishedCall.FunctionName) |> ApiSuccess |> Api |> Cmd.ofMsg
            onts |> PersistentStorage.NewSearchableOntologies |> PersistentStorageMsg |> Cmd.ofMsg
        ]

        nextState, cmds

    | SearchForInsertTermsResponse (termsWithSearchResult) ->
        let finishedCall = {
            currentState.currentCall with
                Status = Successfull
        }
        let nextState = {
            currentCall = ApiState.noCall
            callHistory = finishedCall::currentState.callHistory
        }
        let cmd =
            SpreadsheetInterface.UpdateTermColumnsResponse termsWithSearchResult |> InterfaceMsg |> Cmd.ofMsg
        let loggingCmd =
             ("Debug",sprintf "[ApiSuccess]: Call %s successfull." finishedCall.FunctionName) |> ApiSuccess |> Api |> Cmd.ofMsg
        nextState, Cmd.batch [cmd; loggingCmd]

    //
    | GetAppVersionResponse appVersion ->
        let finishedCall = {
            currentState.currentCall with
                Status = Successfull
        }

        let nextState = {
            currentCall = ApiState.noCall
            callHistory = finishedCall::currentState.callHistory
        }

        let cmds = Cmd.batch [
            ("Debug",sprintf "[ApiSuccess]: Call %s successfull." finishedCall.FunctionName) |> ApiSuccess |> Api |> Cmd.ofMsg
            appVersion |>PersistentStorage. UpdateAppVersion |> PersistentStorageMsg |> Cmd.ofMsg
        ]

        nextState, cmds

open Dev
open Messages

let handleApiMsg (apiMsg:ApiMsg) (currentState:ApiState) : ApiState * Cmd<Messages.Msg> =
    match apiMsg with
    | ApiError e ->
        
        let failedCall = {
            currentState.currentCall with
                Status = Failed (e.GetPropagatedError())
        }

        let nextState = {
            currentCall = ApiState.noCall
            callHistory = failedCall::currentState.callHistory
        }
        let batch = Cmd.batch [
            let modalName = "GenericError"
            Cmd.ofEffect(fun _ -> Modals.Controller.renderModal(modalName, Modals.ErrorModal.errorModal(e)))
            curry GenericLog Cmd.none ("Error",sprintf "[ApiError]: Call %s failed with: %s" failedCall.FunctionName (e.GetPropagatedError())) |> DevMsg |> Cmd.ofMsg
        ]

        nextState, batch

    | ApiSuccess (level,logMsg) ->
        currentState, curry GenericLog Cmd.none (level,logMsg) |> DevMsg |> Cmd.ofMsg

    | Request req ->
        handleApiRequestMsg req currentState
    | Response res ->
        handleApiResponseMsg res currentState

let handlePersistenStorageMsg (persistentStorageMsg: PersistentStorage.Msg) (currentState:PersistentStorageState) : PersistentStorageState * Cmd<Msg> =
    match persistentStorageMsg with
    | PersistentStorage.NewSearchableOntologies onts ->
        let nextState = {
            currentState with
                SearchableOntologies    = onts |> Array.map (fun ont -> ont.Name |> SorensenDice.createBigrams, ont)
                HasOntologiesLoaded     = true
        }

        nextState,Cmd.none
    | PersistentStorage.UpdateAppVersion appVersion ->
        let nextState = {
            currentState with
                AppVersion = appVersion
        }
        nextState,Cmd.none
    | PersistentStorage.UpdateShowSidebar show ->
        {currentState with ShowSideBar = show}, Cmd.none

let handleBuildingBlockDetailsMsg (topLevelMsg:BuildingBlockDetailsMsg) (currentState: BuildingBlockDetailsState) : BuildingBlockDetailsState * Cmd<Msg> =
    match topLevelMsg with
    // Client
    | UpdateBuildingBlockValues nextValues ->
        let nextState = {
            currentState with
                BuildingBlockValues = nextValues
        }
        nextState, Cmd.none
    | UpdateCurrentRequestState nextRequState ->
        let nextState = {
            currentState with
                CurrentRequestState = nextRequState
        }
        nextState, Cmd.none
    // Server
    | GetSelectedBuildingBlockTermsRequest searchTerms ->
        let nextState = {
            currentState with
                CurrentRequestState = RequestBuildingBlockInfoStates.RequestDataBaseInformation
        }
        let cmd =
            Cmd.OfAsync.either
                Api.api.getTermsByNames
                searchTerms
                (GetSelectedBuildingBlockTermsResponse >> BuildingBlockDetails)
                (fun x ->
                    Msg.Batch [
                        curry GenericError Cmd.none x |> DevMsg
                        UpdateCurrentRequestState Inactive |> BuildingBlockDetails
                    ]
                )
        nextState, cmd
    | GetSelectedBuildingBlockTermsResponse searchTermResults ->
        let nextState = {
            BuildingBlockValues = searchTermResults
            CurrentRequestState = Inactive
        }
        let cmd = Cmd.ofEffect(fun dispatch ->
            Modals.Controller.renderModal("BuildingBlockDetails", Modals.BuildingBlockDetailsModal.buildingBlockDetailModal(nextState, dispatch))
        )
        nextState, cmd
            
let handleTopLevelMsg (topLevelMsg:TopLevelMsg) (currentModel: Model) : Model * Cmd<Msg> =
    match topLevelMsg with
    // Client
    | CloseSuggestions ->
        let nextModel = {
            currentModel with
                AddBuildingBlockState = {
                    currentModel.AddBuildingBlockState with
                        ShowUnit2TermSuggestions = false
                }
        }
        nextModel, Cmd.none

let update (msg : Msg) (model : Model) : Model * Cmd<Msg> =
    let innerUpdate (msg: Msg) (currentModel: Model) =
        match msg with
        | DoNothing -> currentModel,Cmd.none
        | Run callback -> 
            callback()
            model, Cmd.none
        | UpdateHistory next -> {model with History = next}, Cmd.none
        | TestMyAPI ->
            let cmd =
                Cmd.OfAsync.either
                    Api.testAPIv1.test
                        ()
                        (curry GenericLog Cmd.none)
                        (curry GenericError Cmd.none)
            currentModel, Cmd.map DevMsg cmd
        | TestMyPostAPI ->
            let cmd =
                Cmd.OfAsync.either
                    Api.testAPIv1.postTest
                        ("instrument Mod")
                        (curry GenericLog Cmd.none)
                        (curry GenericError Cmd.none)
            currentModel, Cmd.map DevMsg cmd
        | Batch msgSeq ->
            let cmd =
                Cmd.batch [
                    yield!
                        msgSeq |> Seq.map Cmd.ofMsg
                ]
            currentModel, cmd
        | UpdatePageState (pageOpt:Route option) ->
            let nextPageState =
                match pageOpt with
                | Some page -> {
                    currentModel.PageState with
                        CurrentPage = page
                    }
                | None -> {
                    currentModel.PageState with
                        CurrentPage = Route.BuildingBlock
                    }
            let nextModel = {
                currentModel with
                    PageState = nextPageState
            }
            nextModel, Cmd.none
        | UpdateIsExpert b ->
            let nextPageState = {
                currentModel.PageState with
                    IsExpert = b
            }
            let nextModel = {
                currentModel with
                    PageState = nextPageState
            }
            nextModel, Cmd.none        
        // does not work due to office.js ->
        // https://stackoverflow.com/questions/42642863/office-js-nullifies-browser-history-functions-breaking-history-usage
        //| Navigate route ->
        //    currentModel, Navigation.newUrl (Routing.Route.toRouteUrl route)
        | Bounce (delay, bounceId, msgToBounce) ->

            let (debouncerModel, debouncerCmd) =
                currentModel.DebouncerState
                |> Debouncer.bounce delay bounceId msgToBounce

            let nextModel = {
                currentModel with
                    DebouncerState = debouncerModel
            }

            nextModel,Cmd.map DebouncerSelfMsg debouncerCmd

        | DebouncerSelfMsg debouncerMsg ->
            let nextDebouncerState, debouncerCmd =
                Debouncer.update debouncerMsg currentModel.DebouncerState

            let nextModel = {
                currentModel with
                    DebouncerState = nextDebouncerState
            }
            nextModel, debouncerCmd

        | OfficeInteropMsg excelMsg ->
            let nextModel,nextCmd = Update.OfficeInterop.update currentModel excelMsg
            nextModel,nextCmd

        | SpreadsheetMsg msg ->
            let nextState, nextModel, nextCmd = Update.Spreadsheet.update currentModel.SpreadsheetModel currentModel msg
            let nextModel' = {nextModel with SpreadsheetModel = nextState}
            nextModel', nextCmd

        | InterfaceMsg msg ->
            Update.Interface.update currentModel msg

        | TermSearchMsg termSearchMsg ->
            let nextTermSearchState,nextCmd =
                currentModel.TermSearchState
                |> TermSearch.update termSearchMsg

            let nextModel = {
                currentModel with
                    TermSearchState = nextTermSearchState
            }
            nextModel,nextCmd
        | AdvancedSearchMsg msg ->
            let nextModel, cmd = AdvancedSearch.update msg model

            nextModel, cmd

        | DevMsg msg ->
            let nextDevState,nextCmd = currentModel.DevState |> Dev.update msg
        
            let nextModel = {
                currentModel with
                    DevState = nextDevState
            }
            nextModel,nextCmd

        | Api apiMsg ->
            let nextApiState,nextCmd = currentModel.ApiState |> handleApiMsg apiMsg

            let nextModel = {
                currentModel with
                    ApiState = nextApiState
            }
            nextModel,nextCmd

        | PersistentStorageMsg persistentStorageMsg ->
            let nextPersistentStorageState,nextCmd =
                currentModel.PersistentStorageState
                |> handlePersistenStorageMsg persistentStorageMsg

            let nextModel = {
                currentModel with
                    PersistentStorageState = nextPersistentStorageState
            }

            nextModel,nextCmd

        | FilePickerMsg filePickerMsg ->
            let nextFilePickerState,nextCmd =
                currentModel.FilePickerState
                |> FilePicker.update filePickerMsg

            let nextModel = {
                currentModel with
                    FilePickerState = nextFilePickerState
            }

            nextModel,nextCmd

        | BuildingBlockMsg addBuildingBlockMsg ->
            let nextAddBuildingBlockState,nextCmd = 
                currentModel.AddBuildingBlockState
                |> BuildingBlock.Core.update addBuildingBlockMsg

            let nextModel = {
                currentModel with
                    AddBuildingBlockState = nextAddBuildingBlockState
                }
            nextModel, nextCmd

        | ProtocolMsg fileUploadJsonMsg ->
            let nextFileUploadJsonState, nextCmd =
                currentModel.ProtocolState
                |> Protocol.update fileUploadJsonMsg

            let nextModel = {
                currentModel with
                    ProtocolState = nextFileUploadJsonState
                }
            nextModel, nextCmd

        | BuildingBlockDetails buildingBlockDetailsMsg ->
            let nextState, nextCmd =
                currentModel.BuildingBlockDetailsState
                |> handleBuildingBlockDetailsMsg buildingBlockDetailsMsg

            let nextModel = {
                currentModel with
                    BuildingBlockDetailsState = nextState
                }
            nextModel, nextCmd

        //| SettingsXmlMsg msg ->
        //    let nextState, nextCmd =
        //        currentModel.SettingsXmlState
        //        |> SettingsXml.update msg
        //    let nextModel = {
        //        currentModel with
        //            SettingsXmlState = nextState
        //    }
        //    nextModel, nextCmd

        | CytoscapeMsg msg ->
            let nextState, nextModel0, nextCmd =
                Cytoscape.Update.update msg currentModel.CytoscapeModel currentModel 
            let nextModel =
                {nextModel0 with
                    CytoscapeModel = nextState}
            nextModel, nextCmd

        | JsonExporterMsg msg ->
            let nextModel, nextCmd = currentModel |> JsonExporter.Core.update msg
            nextModel, nextCmd

        | TemplateMetadataMsg msg ->
            let nextModel, nextCmd = currentModel |> TemplateMetadata.Core.update msg
            nextModel, nextCmd

        | DagMsg msg ->
            let nextModel, nextCmd = currentModel |> Dag.Core.update msg
            nextModel, nextCmd

        | TopLevelMsg msg ->
            let nextModel, nextCmd =
                handleTopLevelMsg msg currentModel

            nextModel, nextCmd

    /// This function is used to determine which msg should be logged to activity log.
    /// The function is exception based, so msg which should not be logged needs to be added here.
    let matchMsgToLog (msg: Msg) =
        match msg with
        | Bounce _ | DevMsg _ | UpdatePageState _ -> false
        | _ -> true

    let logg (msg:Msg) (model: Model) : Model =
        if matchMsgToLog msg then
            let l = 62
            let txt = $"{msg.ToString()}"
            let txt = if txt.Length > l then txt.Substring(0, l) +  ".." else txt
            let nextState = {
                model.DevState with
                    Log = (LogItem.ofStringNow "Info" txt)::model.DevState.Log
            }
            let nextModel = {
                model with DevState = nextState
            }
            nextModel
        else
            model

    logg msg model
    |> innerUpdate msg 