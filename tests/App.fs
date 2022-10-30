module App

open Elmish
open Feliz
open Fable.Core
open Browser
open Elmish.UrlParser
open Elmish.HMR
open Elmish.Navigation

type Bundler =
    | Webpack
    | Vite
    | Parcel
    | Unknown

[<RequireQualifiedAccess>]
type Page =
    | Home
    | About

type Model =
    {
        CurrentRoute : Router.Route option
        ActivePage : Page
        Value : int
        Reloads : int
        Bundler : Bundler
    }

type Msg =
    // DU parameter needed: https://github.com/fable-compiler/Fable/issues/3102
    | Tick of unit
    | Loaded of unit

let private setRoute (result: Option<Router.Route>) (model : Model) =
    let model = { model with CurrentRoute = result }

    match result with
    | None ->
        let requestedUrl = Browser.Dom.window.location.href

        JS.console.error("Error parsing url: " + requestedUrl)

        { model with
            ActivePage = Page.Home
        }
        , Cmd.none

    | Some route ->
        match route with
        | Router.Home ->
            { model with
                ActivePage = Page.Home
            }
            , Cmd.none

        | Router.About ->
            { model with
                ActivePage = Page.About
            }
            , Cmd.none


let private init (optRoute : Router.Route option) =
    let bundler =
        match int window.location.port with
        | 3000 -> Webpack
        | 3001 -> Parcel
        | 3002 -> Vite
        | _ -> Unknown

    {
        CurrentRoute = None
        ActivePage = Page.Home
        Value = 0
        Reloads = -1 // initial load doesn't count
        Bundler = bundler
    }
    |> setRoute optRoute

let private update (msg : Msg) (model : Model) =
    match msg with
    | Tick _ ->
        { model with
            Value = model.Value + 1
        }
        , Cmd.none
    | Loaded _ ->
        { model with
            Reloads = model.Reloads + 1
        }
        , Cmd.none

// We are not using Hooks for the Tick system so Elmish HMR
// support is tested and not React HMR or React fast refresh

module Sub =
    let tick intervalMs onTick onStarted =
        let subId = ["tick"]
        let start dispatch =
            let intervalId = JS.setInterval (fun () -> dispatch onTick) intervalMs
            // setTimeout needed: https://github.com/elmish/elmish/issues/256
            JS.setTimeout (fun () -> dispatch onStarted) 1 |> ignore
            { new System.IDisposable with
                member _.Dispose() = JS.clearInterval intervalId }
        subId, start

let private subscribe model =
    // sub doesn't change if model changes, is running as long as app runs
    // so we can use it to know when app reloads
    [ Sub.tick 1000 (Tick ()) (Loaded ()) ]

let private liveCounter model =
    Html.section [
        prop.classes ["section"]
        prop.children [
            Html.div [
                prop.classes ["has-text-centered"]
                prop.children [
                    Html.div [
                        Html.text "Application is running since "
                        Html.text (string model.Value)
                        Html.text " seconds with "
                        Html.text (string model.Reloads)
                        Html.text " reloads"
                    ]

                    Html.br [ ]

                    Html.text "Change me and check that the timer has not been reset"
                ]
            ]
        ]
    ]

let private navbar (model : Model) =
    Html.nav [
        prop.classes ["navbar"; "is-info"]
        prop.children [
            Html.div [
                prop.classes ["navbar-menu"]
                prop.children [
                    Html.div [
                        prop.classes ["navbar-start"]
                        prop.children [
                            Html.a [
                                prop.classes ["navbar-item"]
                                Router.href Router.Home

                                prop.text "Home"
                            ]

                            Html.a [
                                prop.classes ["navbar-item"]
                                Router.href Router.About

                                prop.text "About"
                            ]
                        ]
                    ]
                ]
            ]
        ]
    ]

let private renderActivePage (page : Page) =
    let content =
        match page with
        | Page.Home ->
            Html.p [
                prop.classes ["has-text-centered"]

                prop.children [
                    Html.text "This is the home page"
                ]
            ]

        | Page.About ->
            Html.p [
                prop.classes ["has-text-centered"]

                prop.children [
                    Html.text "This is the about page"
                ]
            ]

    Html.section [
        prop.classes ["section"]
        prop.children [
            content
        ]
    ]

let private renderBundlerInformation (bundler : Bundler) =
    let bundlerText =
        match bundler with
        | Webpack -> "Webpack"
        | Parcel -> "Parcel"
        | Vite -> "Vite"
        | Unknown -> "Unknown"

    Html.section [
        prop.classes ["section"]
        prop.children [
            Html.p [
                prop.classes ["has-text-centered"]

                prop.children [
                    Html.text "This is page is running using: "
                    Html.text bundlerText
                ]
            ]
        ]
    ]

let private lazyViewTest (_value : int) =
    Html.section [
        prop.classes ["section"]
        prop.children [
            Html.div [
                prop.classes ["has-text-centered"]

                prop.children [
                    Html.p "Change this text and see that it has been changed after HMR being applied"
                    Html.br [ ]
                    Html.p "If HMR was not supported this text would not change until a full refresh of the page"
                ]
            ]
        ]
    ]

let private view (model : Model) (dispatch : Dispatch<Msg>) =
    Html.div [
        navbar model

        renderBundlerInformation model.Bundler

        Html.hr [ ]

        renderActivePage model.ActivePage

        Html.hr [ ]

        Html.div [
            prop.classes ["container"]
            prop.children [
                liveCounter model
            ]
        ]

        Html.hr [ ]

        // We are passing a stable value
        // because we want to test that this is the HMR counter
        // increment which cause the changes and not the passed value
        lazyView lazyViewTest 2

        Html.hr [ ]
    ]



Program.mkProgram init update view
|> Program.withSubscription subscribe
|> Program.toNavigable (parseHash Router.pageParser) setRoute
|> Program.withReactBatched "root"
|> Program.run
