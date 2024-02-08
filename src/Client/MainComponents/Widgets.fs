﻿namespace MainComponents

open Feliz
open Feliz.Bulma
open Browser.Types

open LocalStorage.Widgets

module InitExtensions =

    type Rect with

        static member initSizeFromPrefix(prefix: string) =
            match Size.load prefix with
            | Some p -> Some p
            | None -> None

        static member initPositionFromPrefix(prefix: string) =
            match Position.load prefix with
            | Some p -> Some p
            | None -> None


open InitExtensions

open Fable.Core
open Fable.Core.JsInterop

module MoveEventListener =

    open Fable.Core.JsInterop

    let calculatePosition (element:IRefValue<HTMLElement option>) (startPosition: Rect) = fun (e: Event) ->
        let e : MouseEvent = !!e
        let maxX = Browser.Dom.window.innerWidth - element.current.Value.offsetWidth;
        let tempX = int e.clientX - startPosition.X
        let newX = System.Math.Min(System.Math.Max(tempX,0),int maxX)
        let maxY = Browser.Dom.window.innerHeight - element.current.Value.offsetHeight;
        let tempY = int e.clientY - startPosition.Y
        let newY = System.Math.Min(System.Math.Max(tempY,0),int maxY)
        {X = newX; Y = newY}

    let onmousemove (element:IRefValue<HTMLElement option>) (startPosition: Rect) setPosition = fun (e: Event) ->
        let nextPosition = calculatePosition element startPosition e
        setPosition (Some nextPosition)

    let onmouseup (prefix,element:IRefValue<HTMLElement option>) onmousemove = 
        Browser.Dom.document.removeEventListener("mousemove", onmousemove)
        if element.current.IsSome then
            let rect = element.current.Value.getBoundingClientRect()
            let position = {X = int rect.left; Y = int rect.top}
            Position.write(prefix,position)

module ResizeEventListener =

    open Fable.Core.JsInterop

    let onmousemove (startPosition: Rect) (startSize: Rect) setSize = fun (e: Event) ->
        let e : MouseEvent = !!e
        let width = int e.clientX - startPosition.X + startSize.X
        //let height = int e.clientY - startPosition.Y + startSize.Y
        setSize (Some {X = width; Y = startSize.Y})

    let onmouseup (prefix, element: IRefValue<HTMLElement option>) onmousemove = 
        Browser.Dom.document.removeEventListener("mousemove", onmousemove)
        if element.current.IsSome then 
            Size.write(prefix,{X = int element.current.Value.offsetWidth; Y = int element.current.Value.offsetHeight})

module Elements =
    let resizeElement (content: ReactElement)  =
        Html.div [
            prop.style [style.cursor.northWestSouthEastResize; style.border(1, borderStyle.solid, "black")]
            prop.children content
        ]

    let helpExtendButton (extendToggle: unit -> unit) =
        Bulma.help [
            prop.className "is-flex"
            prop.children [
                Html.a [
                    prop.text "Help"; 
                    prop.style [style.marginLeft length.auto; style.userSelect.none]
                    prop.onClick (fun e -> e.preventDefault(); e.stopPropagation(); extendToggle())
                ]
            ]
        ]

type Widgets =

    [<ReactComponent>]
    static member Base(content: ReactElement, prefix: string, rmv: MouseEvent -> unit, ?help: ReactElement) =
        let position, setPosition = React.useState(fun _ -> Rect.initPositionFromPrefix prefix)
        let size, setSize = React.useState(fun _ -> Rect.initSizeFromPrefix prefix)
        let helpIsActive, setHelpIsActive = React.useState(false)
        let element = React.useElementRef()
        let resizeElement (content: ReactElement) =
            Bulma.modalCard [
                prop.ref element
                prop.onMouseDown(fun e ->  // resize
                    e.preventDefault()
                    e.stopPropagation()
                    let startPosition = {X = int e.clientX; Y = int e.clientY}
                    let startSize = {X = int element.current.Value.offsetWidth; Y = int element.current.Value.offsetHeight}
                    let onmousemove = ResizeEventListener.onmousemove startPosition startSize setSize
                    let onmouseup = fun e -> ResizeEventListener.onmouseup (prefix, element) onmousemove
                    Browser.Dom.document.addEventListener("mousemove", onmousemove)
                    let config = createEmpty<AddEventListenerOptions>
                    config.once <- true
                    Browser.Dom.document.addEventListener("mouseup", onmouseup, config)
                )
                prop.style [
                    style.cursor.eastWestResize; style.display.flex
                    style.padding(2); style.overflow.visible
                    style.position.fixedRelativeToWindow
                    if size.IsSome then
                        style.width size.Value.X
                    if position.IsNone then
                        //style.transform.translate (length.perc -50,length.perc -50)
                        style.top (length.perc 20); style.left (length.perc 20); 
                    else
                        style.top position.Value.Y; style.left position.Value.X; 
                ]
                prop.children content
            ]
        resizeElement <| Html.div [
            prop.onMouseDown(fun e -> e.stopPropagation())
            prop.style [style.cursor.defaultCursor]
            prop.children [
                Bulma.modalCardHead [
                    prop.onMouseDown(fun e -> // move
                        e.preventDefault()
                        e.stopPropagation()
                        let x = e.clientX - element.current.Value.offsetLeft
                        let y = e.clientY - element.current.Value.offsetTop;
                        let startPosition = {X = int x; Y = int y}
                        let onmousemove = MoveEventListener.onmousemove element startPosition setPosition
                        let onmouseup = fun e -> MoveEventListener.onmouseup (prefix, element) onmousemove
                        Browser.Dom.document.addEventListener("mousemove", onmousemove)
                        let config = createEmpty<AddEventListenerOptions>
                        config.once <- true
                        Browser.Dom.document.addEventListener("mouseup", onmouseup, config)
                    )
                    prop.style [style.cursor.move]
                    prop.children [
                        Bulma.modalCardTitle Html.none
                        Bulma.delete [ prop.onClick rmv ]
                    ]
                ]
                Bulma.modalCardBody [
                    prop.style [style.overflow.inheritFromParent]
                    prop.children [
                        content
                        if help.IsSome then Elements.helpExtendButton (fun _ -> setHelpIsActive (not helpIsActive))
                    ]
                ]
                Bulma.modalCardFoot [
                    prop.style [style.padding 5]
                    if help.IsSome then
                        prop.children [
                            Bulma.content [
                                prop.className "widget-help-container"
                                prop.style [style.overflow.hidden; if not helpIsActive then style.display.none; ]
                                prop.children [
                                    help.Value
                                ]
                            ]
                        ]
                ]
            ]
        ]
        
    static member BuildingBlock (model, dispatch, rmv: MouseEvent -> unit) =
        let content = BuildingBlock.SearchComponent.Main model dispatch
        let help = Html.div [
            Html.p "Add a new Building Block."
            Html.ul [
                Html.li "If a cell is selected, a new Building Block is added to the right of the selected cell."
                Html.li "If no cell is selected, a new Building Block is appended at the right end of the table."
            ]
        ]
        let prefix = BuildingBlockWidgets
        Widgets.Base(content, prefix, rmv, help)
        

    [<ReactComponent>]
    static member Templates (model: Messages.Model, dispatch, rmv: MouseEvent -> unit) =
        let templates, setTemplates = React.useState(model.ProtocolState.Templates)
        React.useEffectOnce(fun _ -> Messages.Protocol.GetAllProtocolsRequest |> Messages.ProtocolMsg |> dispatch)
        React.useEffect((fun _ -> setTemplates model.ProtocolState.Templates), [|box model.ProtocolState.Templates|])
        let selectContent() = 
            [
                Protocol.Search.FileSortElement(templates, setTemplates, model, dispatch)
                Protocol.Search.Component (templates, model, dispatch, length.px 300)
            ]
        let insertContent() =
            [
                Bulma.field.div [
                    Protocol.Core.TemplateFromDB.addFromDBToTableButton model dispatch
                ]
                Bulma.field.div [
                    prop.style [style.maxHeight (length.px 300); style.overflow.auto]
                    prop.children [
                        Protocol.Core.TemplateFromDB.displaySelectedProtocolEle model dispatch
                    ]
                ]
            ]
        let content = 
            let switchContent = if model.ProtocolState.TemplateSelected.IsNone then selectContent() else insertContent()
            Html.div [
                prop.children switchContent
            ]
        
        let help = Protocol.Search.InfoField()
        let prefix = TemplatesWidgets
        Widgets.Base(content, prefix, rmv, help)
