module ProtocolInsertView

open System

open Fulma
open Fable
open Fable.React
open Fable.React.Props
open Fable.FontAwesome
//open Fable.Core.JS
open Fable.Core.JsInterop

//open ISADotNet

open Model
open Messages
open Browser.Types
open Fulma.Extensions.Wikiki

open Shared
open ISADotNetHelpers

// https://www.growingwiththeweb.com/2016/07/enabling-pull-requests-on-github-wikis.html

let isViableISADotNetProcess (isaProcess:ISADotNet.Process) =

    // The following comment was written with another function in mind, but can be used as base for documentation.

    let isExistingChecks =
        let hasExecProtocol         = "executesProtocol", isaProcess.ExecutesProtocol.IsSome
        let hasProtocolParams       = "parameterValues", isaProcess.ParameterValues.IsSome
        let hasExecProtocolParams   =
            "executesProtocol.parameters",
                if isaProcess.ExecutesProtocol.IsSome then
                    isaProcess.ExecutesProtocol.Value.Parameters.IsSome
                else
                    false
        let hasAnnoTSRTAN =
            isaProcess.ExecutesProtocol.Value.Parameters.Value
            |> List.map (fun p ->
                if p.ParameterName.IsSome then
                    "executesProtocol.parameters.Anno/TSR/TAN",
                    p.ParameterName.Value.Name.IsSome
                    && p.ParameterName.Value.TermAccessionNumber.IsSome
                    && p.ParameterName.Value.TermSourceREF.IsSome
                else
                    "executesProtocol.parameters.Anno/TSR/TAN",
                    false
            )
        let hasAnnoTSRTAN2 =
            isaProcess.ParameterValues.Value
            |> List.map (fun x ->
                if x.Category.IsSome then
                    if x.Category.Value.ParameterName.IsSome then
                        "parameterValues.category.parameterName.Anno/TST/TAN",
                        x.Category.Value.ParameterName.Value.Name.IsSome
                        && x.Category.Value.ParameterName.Value.TermSourceREF.IsSome
                        && x.Category.Value.ParameterName.Value.TermAccessionNumber.IsSome
                    else
                        "parameterValues.category.parameterName.Anno/TST/TAN",
                        false
                else
                    "parameterValues.category.parameterName.Anno/TST/TAN",
                    false
            )
        let hasParameterValueValue =
            isaProcess.ParameterValues.Value
            |> List.map (fun x ->
                "parameterValues.Value",
                x.Value.IsSome
            )
        [|hasExecProtocol; hasProtocolParams; hasExecProtocolParams; yield! hasAnnoTSRTAN; yield! hasAnnoTSRTAN; yield! hasParameterValueValue|]
        |> Collections.Array.filter (fun (param,isExisting) ->
            isExisting = false
        )
    if isExistingChecks |> Collections.Array.isEmpty then
        let execParams = isaProcess.ExecutesProtocol.Value.Parameters.Value
        let paramValuePairs = isaProcess.ParameterValues.Value
        /// As we want a very controlled environment we establish this failsafe for now. This can be changed later on.
        let isSameLength = execParams.Length = paramValuePairs.Length
        /// As we want a very controlled environment we establish this failsafe for now. This can be changed later on.
        let hasSameEntrys =
            paramValuePairs
            |> List.choose (fun paramValuePair ->
                let param = paramValuePair.Category.Value.ParameterName.Value
                execParams |> List.tryFind (fun execParam ->
                    execParam.ParameterName.Value.Name                      = param.Name
                    && execParam.ParameterName.Value.TermAccessionNumber    = param.TermAccessionNumber
                    && execParam.ParameterName.Value.TermSourceREF          = param.TermSourceREF
                )
            )
        if not isSameLength then
            false, Some <| sprintf "Process.ExecutesProtocol.Parameters and Process.ParameterValues has not the same number of items: %i - %i" execParams.Length paramValuePairs.Length
        elif hasSameEntrys.Length <> paramValuePairs.Length then
            false, Some <| sprintf "Process.ExecutesProtocol.Parameters and Process.ParameterValues has different values in (category.)parameterName.annotationValue."
        else
            true, None
    else
        false, Some <| sprintf "Process contains missing values: %A" (isExistingChecks |> Collections.Array.map fst)

let paramValuePairElement (ppv:ISADotNet.ProcessParameterValue) =
    Table.table [Table.IsFullWidth; Table.IsBordered][
        thead [][
            tr [][
                th [Style [Width "50%"]] [
                    str (annotationValueToString ppv.Category.Value.ParameterName.Value.Name.Value)
                ]
                th [][
                    str (termAccessionReduce ppv.Category.Value.ParameterName.Value.TermAccessionNumber.Value)
                ]
            ]
        ]
        tbody [][
            tr [Style [Width "50%"] ][
                let isOntology = valueIsOntology ppv.Value.Value
                td [][
                    str (
                        if isOntology.IsSome then
                            isOntology.Value.Name
                        elif ppv.Unit.IsSome then
                            let unitName = ppv.Unit.Value.Name.Value |> annotationValueToString
                            let value = valueToString ppv.Value.Value
                            sprintf "%s %s" value unitName
                        else
                            valueToString ppv.Value.Value
                    )
                ]
                td [][
                    str (
                        if isOntology.IsSome then
                            isOntology.Value.TermAccession
                        elif ppv.Unit.IsSome then
                            ppv.Unit.Value.TermAccessionNumber.Value |> termAccessionReduce
                        else
                            valueToString ppv.Value.Value
                    )
                ]
            ]
        ]
    ]

/// only diplayed if model.ProtocolInsertState.ProcessModel.IsSome
let displayProtocolInfoElement isViable (errorMsg:string option) (model:Model) dispatch =

    if not isViable then
        [
            Label.label [Label.Props [Style [Color NFDIColors.Red.Base]]][str "The following errors occured:"]
            str (errorMsg.Value)
        ] 
    else 
        [
            let paramValuePairs = model.ProtocolInsertState.ProcessModel.Value.ParameterValues.Value
            Field.div [][
                yield div [Style [MarginBottom "1rem"]][
                    b [][ str model.ProtocolInsertState.ProcessModel.Value.ExecutesProtocol.Value.Name.Value ]
                    str (sprintf " - Version %s" model.ProtocolInsertState.ProcessModel.Value.ExecutesProtocol.Value.Version.Value)
                ]
                for paramValuePair in paramValuePairs do
                    yield paramValuePairElement paramValuePair
            ]
        ]

let fileUploadButton (model:Model) dispatch id =
    Label.label [Label.Props [Style [Margin "1rem auto"]]][
        Input.input [
            Input.Props [
                Id id
                Type "file"; Style [Display DisplayOptions.None]
                OnChange (fun ev ->
                    let files : FileList = ev.target?files

                    let fileNames =
                        [ for i=0 to (files.length - 1) do yield files.item i ]
                        |> List.map (fun f -> f.slice() )

                    let reader = Browser.Dom.FileReader.Create()

                    reader.onload <- fun evt ->
                        UpdateUploadData evt.target?result |> ProtocolInsert |> dispatch
                                   
                    reader.onerror <- fun evt ->
                        GenericLog ("Error", evt.Value) |> Dev |> dispatch

                    reader.readAsText(fileNames |> List.head)

                    let picker = Browser.Dom.document.getElementById(id)
                    // https://stackoverflow.com/questions/3528359/html-input-type-file-file-selection-event/3528376
                    picker?value <- null
                )
            ]
        ]
        Button.a [Button.Color Color.IsInfo; Button.IsFullWidth][
            str "Upload Process.json"
        ]
    ]

open OfficeInterop.Types.Xml

let addToTableButton isValid (model:Model) dispatch =
    Field.div [] [
        Control.div [] [
            Button.a [
                if isValid then
                    Button.IsActive true
                else
                    Button.Color Color.IsDanger
                    Button.Props [Disabled true]
                Button.IsFullWidth
                Button.Color IsSuccess
                Button.OnClick (fun e ->
                    let preProtocol =
                        let p = model.ProtocolInsertState.ProcessModel.Value
                        let id = p.ExecutesProtocol.Value.Name.Value
                        let version = p.ExecutesProtocol.Value.Version.Value
                        let swateVersion = model.PersistentStorageState.AppVersion
                        GroupTypes.Protocol.create id version swateVersion "" "" []
                    let minBuildingBlockInfos =
                        OfficeInterop.Types.BuildingBlockTypes.MinimalBuildingBlock.ofISADotNetProcess model.ProtocolInsertState.ProcessModel.Value
                        |> List.rev
                    pipeNameTuple2 AddAnnotationBlocks (minBuildingBlockInfos,preProtocol) |> ExcelInterop |> dispatch

                    //for minBuildBlockInfo in minBuildingBlockInfos
                    //    do 
                    //        let msg tableName = AddAnnotationBlock (tableName, minBuildBlockInfo)
                    //        PipeActiveAnnotationTable msg |> ExcelInterop |> dispatch
                )
            ] [
                str "Insert protocol annotation blocks"
            ]
        ]
    ]

let protocolInsertElement (model:Model) dispatch =
    let isViable, errorMsg =
        if model.ProtocolInsertState.ProcessModel.IsSome then
            isViableISADotNetProcess model.ProtocolInsertState.ProcessModel.Value
        else
            false, "" |> Some
    div [
        Style [
            BorderLeft (sprintf "5px solid %s" NFDIColors.Mint.Base)
            //BorderRadius "15px 15px 0 0"
            Padding "0.25rem 1rem"
            MarginBottom "1rem"
        ]
    ] [
        Field.div [Field.Props [Style [
            Width "100%"
        ]]][
            if model.ProtocolInsertState.ProcessModel.IsSome then
                yield! displayProtocolInfoElement isViable errorMsg model dispatch
            else
                Field.div [][
                    str "The protocol building blocks will be shown here"
                ]

            addToTableButton isViable model dispatch
        ]
    ]

let fileUploadViewComponent (model:Model) dispatch =
    let uploadId = "UploadFiles_ElementId"
    form [
        OnSubmit (fun e -> e.preventDefault())
        // https://keycode.info/
        OnKeyDown (fun k -> if k.key = "Enter" then k.preventDefault())
    ] [

        Label.label [Label.Size Size.IsLarge; Label.Props [Style [Color model.SiteStyleState.ColorMode.Accent]]][ str "Protocol driven building block insert"]

        Help.help [][
            b [] [
                str "Upload a "
                a [Href "https://github.com/nfdi4plants/Swate/wiki/Insert-via-Process.json"; Target "_Blank"][ str "process.json" ]
                str " file."
            ]
            str " The building blocks in this file can be group inserted into a Swate table."
            str " In the future these files will be accessible either by "
            a [Href "https://github.com/nfdi4plants/Spawn"; Target "_Blank"] [str "Spawn"]
            str " or offered as download!"
        ]

        fileUploadButton model dispatch uploadId

        Label.label [Label.Size Size.IsSmall; Label.Props [Style [Color model.SiteStyleState.ColorMode.Accent]]] [str "Show uploaded file data."]

        protocolInsertElement model dispatch

        //div [][
        //    str (
        //        let dataStr = model.ProtocolInsertState.ProcessModel
        //        if dataStr.IsNone then "no upload data found" else sprintf "%A" model.ProtocolInsertState.ProcessModel.Value
        //    )
        //]
    ]