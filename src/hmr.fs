namespace Elmish.HMR

open Fable.Core.JsInterop
open Browser
open Elmish

[<RequireQualifiedAccess>]
module Program =

    module Internal =
        type Platform =
            | Browser
            | ReactNative

        let platform =
            match window?navigator?product with
            | "ReactNative" -> ReactNative
            | _ -> Browser

        let tryRestoreState (hot : HMR.IHot) =
            match platform with
            | ReactNative ->
                let hmrState = window?react_native_elmish_hmr_state
                if not (isNull hmrState) then
                    Some hmrState
                else
                    None
            | Browser ->
                let data = hot?data
                if not (isNull data) && not (isNull data?hmrState) then
                    Some data?hmrState
                else
                    None

        let saveState (data : obj) (hmrState : obj) =
            match platform with
            | ReactNative ->
                window?react_native_elmish_hmr_state <- hmrState
            | Browser ->
                data?hmrState <- hmrState

    type Msg<'msg> =
        | UserMsg of 'msg
        | Stop

    type Model<'model> =
        | Inactive
        | Active of 'model

    /// Start the dispatch loop with `'arg` for the init() function.
    let inline runWith (arg: 'arg) (program: Program<'arg, 'model, 'msg, 'view>) =
#if !DEBUG
        Program.runWith arg program
#else
        let mutable hmrState : obj = null
        let hot = HMR.``module``.hot

        if not (isNull hot) then
            window?Elmish_HMR_Count <-
                if isNull window?Elmish_HMR_Count then
                    0
                else
                    window?Elmish_HMR_Count + 1

            hot.accept() |> ignore

            match Internal.tryRestoreState hot with
            | Some previousState ->
                hmrState <- previousState
            | None -> ()

        let map (model, cmd) =
            model, cmd |> Cmd.map UserMsg

        let mapUpdate update (msg : Msg<'msg>) (model : Model<'model>) =
            let newModel,cmd =
                match msg with
                    | UserMsg msg ->
                        match model with
                        | Inactive -> model, Cmd.none
                        | Active userModel ->
                            let newModel, cmd = update msg userModel
                            Active newModel, cmd

                    | Stop ->
                        Inactive, Cmd.none
                    |> map

            hmrState <- newModel
            newModel,cmd

        let createModel (model, cmd) =
            Active model, cmd

        let mapInit init =
            if isNull hmrState then
                init >> map >> createModel
            else
                (fun _ -> unbox<Model<_>> hmrState, Cmd.none)

        let mapSetState setState (model : Model<'model>) dispatch =
            match model with
            | Inactive -> ()
            | Active userModel ->
                setState userModel (UserMsg >> dispatch)

        let hmrSubscription =
            let handler dispatch =
                if not (isNull hot) then
                    hot.dispose(fun data ->
                        Internal.saveState data hmrState

                        dispatch Stop
                    )
            [ handler ]

        let mapSubscribe subscribe model =
            match model with
            | Inactive -> Cmd.none
            | Active userModel ->
                Cmd.batch [ subscribe userModel |> Cmd.map UserMsg
                            hmrSubscription ]

        let mapView view =
            // This function will never be executed because we are using a local reference to access `program.view`.
            // For example,
            // ```fs
            // let withReactUnoptimized placeholderId (program: Program<_,_,_,_>) =
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
                    view userModel (UserMsg >> dispatch)

        program
        |> Program.map mapInit mapUpdate mapView mapSetState mapSubscribe
        |> Program.runWith arg
#endif

    /// Start the dispatch loop with `unit` for the init() function.
    let inline run (program: Program<unit, 'model, 'msg, 'view>) =
#if !DEBUG
        Program.run program
#else
        runWith () program
#endif

    (*
        Shadow: Fable.Elmish.Navigation
    *)

    let inline toNavigable
        (parser : Navigation.Parser<'a>)
        (urlUpdate : 'a->'model->('model * Cmd<'msg>))
        (program : Program<'a,'model,'msg,'view>) =
#if !DEBUG
        Navigation.Program.toNavigable parser urlUpdate program
#else
        let onLocationChange (dispatch : Dispatch<_ Navigation.Navigable>) =
            if not (isNull HMR.``module``.hot) then
                // parcel doesn't implement webpack's status() function on its hot modules
                if not (isNull HMR.``module``.hot?status) then
                    if HMR.``module``.hot.status() <> HMR.Idle then
                        Navigation.Program.Internal.unsubscribe ()

            Navigation.Program.Internal.subscribe dispatch

        Navigation.Program.Internal.toNavigableWith parser urlUpdate program onLocationChange
#endif

    (*
        Shadow: Fable.Elmish.React
    *)

    let inline withReactBatched placeholderId (program: Program<_,_,_,_>) =
#if !DEBUG
        Elmish.React.Program.withReactBatched placeholderId program
#else
        Elmish.React.Program.Internal.withReactBatchedUsing lazyView2With placeholderId program
#endif

    let inline withReactSynchronous placeholderId (program: Program<_,_,_,_>) =
#if !DEBUG
        Elmish.React.Program.withReactSynchronous placeholderId program
#else
        Elmish.React.Program.Internal.withReactSynchronousUsing lazyView2With placeholderId program
#endif

    let inline withReactHydrate placeholderId (program: Program<_,_,_,_>) =
#if !DEBUG
        Elmish.React.Program.withReactHydrate placeholderId program
#else
        Elmish.React.Program.Internal.withReactHydrateUsing lazyView2With placeholderId program
#endif

    [<System.Obsolete("Use withReactBatched")>]
    let inline withReact placeholderId (program: Program<_,_,_,_>) =
#if !DEBUG
        Elmish.React.Program.withReactBatched placeholderId program
#else
        Elmish.React.Program.Internal.withReactBatchedUsing lazyView2With placeholderId program
#endif

    [<System.Obsolete("Use withReactSynchronous")>]
    let inline withReactUnoptimized placeholderId (program: Program<_,_,_,_>) =
#if !DEBUG
        Elmish.React.Program.withReactSynchronous placeholderId program
#else
        Elmish.React.Program.Internal.withReactSynchronousUsing lazyView2With placeholderId program
#endif
