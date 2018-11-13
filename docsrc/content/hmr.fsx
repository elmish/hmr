(*** hide ***)
#I "../../src/bin/Debug/netstandard1.6"
#r "Fable.Core.dll"
#r "Fable.Elmish.dll"

(**
*)
namespace Elmish.HMR

open Elmish
open Elmish.Browser
open Fable.Import

[<RequireQualifiedAccess>]
module Program =

    type HMRMsg<'msg> =
        | UserMsg of 'msg
        | Reload

    type HMRModel<'model> =
        { HMRCount : int
          UserModel : 'model }

    let mutable hmrState : obj = null

    let inline withHMR (program:Elmish.Program<'arg, 'model, 'msg, 'view>) =

        if not (isNull HMR.``module``.hot) then
            HMR.``module``.hot.accept() |> ignore

        let map (model, cmd) =
            model, cmd |> Cmd.map UserMsg

        let update msg model =
            let newModel,cmd =
                match msg with
                | UserMsg msg ->
                    let newModel, cmd = program.update msg model.UserModel
                    { model with UserModel = newModel }, cmd
                | Reload ->
                    { model with HMRCount = model.HMRCount + 1 }, Cmd.none
                |> map

            hmrState <- newModel
            // Store the state
            newModel, cmd

        let createModel (model, cmd) =
            { HMRCount = 0
              UserModel = model }, cmd

        let init =
            if isNull hmrState then
                program.init >> map >> createModel
            else
                (fun _ -> unbox<HMRModel<_>> hmrState, Cmd.ofMsg Reload )

        let subs model =
            Cmd.batch [ program.subscribe model.UserModel |> Cmd.map UserMsg ]

        { init = init
          update = update
          subscribe = subs
          onError = program.onError
          setState = fun model dispatch -> program.setState model.UserModel (UserMsg >> dispatch)
          view = fun model dispatch -> program.view model.UserModel (UserMsg >> dispatch) }

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
