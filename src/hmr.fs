namespace Elmish.HMR

open Elmish
open Elmish.Browser
open Fable.Import
open Fable.Core.JsInterop

[<RequireQualifiedAccess>]
module Program =

    type Msg<'msg> =
        | UserMsg of 'msg
        | Stop

    type Model<'model> =
        | Inactive
        | Active of 'model

    #if DEBUG
    /// Start the dispatch loop with `unit` for the init() function.
    let inline run (program: Program<unit, 'model, 'msg, 'view>) =
        let mutable hmrState : obj = null
        let hot = HMR.``module``.hot

        if not (isNull hot) then
            hot.accept() |> ignore

            let data = hot?data
            // Check if we have an old state to restore
            if not (isNull data) && not (isNull data?hmrState) then
                hmrState <- data?hmrState

        let map (model, cmd) =
            model, cmd |> Cmd.map UserMsg

        let update (msg : Msg<'msg>) (model : Model<'model>) =
            let newModel,cmd =
                match msg with
                    | UserMsg msg ->
                        match model with
                        | Inactive -> model, Cmd.none
                        | Active userModel ->
                            let newModel, cmd = program.update msg userModel
                            Active newModel, cmd

                    | Stop ->
                        Inactive, Cmd.none
                    |> map

            hmrState <- newModel
            newModel,cmd

        let setState (model : Model<'model>) dispatch =
            match model with
            | Inactive -> ()
            | Active userModel ->
                program.setState userModel (UserMsg >> dispatch)

        let createModel (model, cmd) =
            Active model, cmd

        let init =
            if isNull hmrState then
                program.init >> map >> createModel
            else
                (fun _ -> unbox<Model<_>> hmrState, Cmd.none)

        let setState (model : Model<'model>) dispatch =
            match model with
            | Inactive -> ()
            | Active userModel ->
                program.setState userModel (UserMsg >> dispatch)

        let hmrSubscription =
            let handler dispatch =
                if not (isNull hot) then
                    hot.addDisposeHandler(fun data ->
                        data?hmrState <- hmrState

                        dispatch Stop
                    )
            [ handler ]

        let subs model =
            match model with
            | Inactive -> Cmd.none
            | Active userModel ->
                Cmd.batch [ program.subscribe userModel |> Cmd.map UserMsg
                            hmrSubscription ]

        let view =
            // This function will never be executed because we are using a local reference to access `program.view`.
            // For example,
            // ```fs
            // let withReactUnoptimized placeholderId (program:Elmish.Program<_,_,_,_>) =
            //     let setState model dispatch =
            //         Fable.Import.ReactDom.render(
            //             lazyView2With (fun x y -> obj.ReferenceEquals(x,y)) program.view model dispatch,
            //                                                                  ^-- Here program is coming from the function parameters and not
            //                                                                      from the last program composition used to run the applicaiton
            //             document.getElementById(placeholderId)
            //         )
            //
            //     { program with setState = setState }
            // ```*)
            fun model dispatch ->
                match model with
                | Inactive ->
                    """
Your are using HMR and this Elmish application has been marked as inactive.

You should not see this message
                    """
                    |> failwith
                | Active userModel ->
                    program.view userModel (UserMsg >> dispatch)

        { init = init
          update = update
          subscribe = subs
          onError = program.onError
          setState = setState
          view = view }

        |> Elmish.Program.runWith ()
    #else
    let inline run (program: Program<unit, 'model, 'msg, 'view>) =
        Elmish.Program.run program
    #endif

    #if DEBUG
    let toNavigable
        (parser : Navigation.Parser<'a>)
        (urlUpdate : 'a->'model->('model * Cmd<'msg>))
        (program : Program<'a,'model,'msg,'view>) =

        let onLocationChange (dispatch : Dispatch<_ Navigation.Navigable>) =
            if not (isNull HMR.``module``.hot) then
                if HMR.``module``.hot.status() <> HMR.Idle then
                    Navigation.Program.Internal.unsubscribe ()

            Navigation.Program.Internal.subscribe dispatch

        Navigation.Program.Internal.toNavigableWith parser urlUpdate program onLocationChange
    #endif
