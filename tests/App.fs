module App

open Elmish
open Feliz
open Feliz.Bulma

type Model =
    {
        Value : int
    }

type Msg =
    | Tick of unit

let private init () =
    {
        Value = 0
    }
    , Cmd.none

let private tick () =
    promise {
        do! Promise.sleep 1000

        return ()
    }

let private update (msg : Msg) (model : Model) =
    match msg with
    | Tick _ ->
        {
            Value = model.Value + 1
        }
        , Cmd.OfPromise.perform tick () Tick

let private view (model : Model) (dispatch : Dispatch<Msg>) =
    Bulma.container [
        Bulma.hero [
            hero.isFullHeight

            prop.children [
                Bulma.heroBody [
                    Bulma.text.div [
                        text.hasTextCentered
                        prop.style [
                            style.width (length.percent 100)
                        ]

                        prop.children [
                            Html.div [
                                Html.text "Application is running since "
                                Html.text (string model.Value)
                                Html.text " seconds"
                            ]

                            Html.br [ ]

                            Html.text "Change me and check that the timer has not been reset"
                        ]
                    ]
                ]
            ]
        ]
    ]

open Elmish.HMR

// Use a subscription to trigger the Tick system
// If we try to trigger the ticks from the Update function
// This doesn't work because dispatch instance available in the waiting tick refers
// to the old programs meaning that the new instance never "Tick"

// Also, we are not using Hooks for the Tick system because we want to test Elmish HMR
// supports and not React HMR or React fast refresh

// Trigger a Tick on program launch to start the timer
let tickSubscription _ =
    Cmd.ofMsg (Tick ())

Program.mkProgram init update view
|> Program.withSubscription tickSubscription
|> Program.withReactBatched "root"
|> Program.run
