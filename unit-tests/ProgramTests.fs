module Tests.ProgramTests

open Fable.Mocha
open Fable.Core.JsInterop
open Fable.React
open Elmish
open Elmish.HMR
open Browser
open Tests.Helpers

/// Minimal helper to create a react element without the Fable.React DSL package.
let private el tag (text: string) : ReactElement =
    ReactBindings.React.createElement(tag, null, [ unbox<ReactElement> text ])

/// Reset the HMR counter to a known value before each test group.
let private resetHmrCount value =
    window?Elmish_HMR_Count <- value

/// Clear any cached root for a given placeholderId.
let private clearCachedRoot (placeholderId: string) =
    window?("__elmish_hmr_root_" + placeholderId) <- null
    window?("__elmish_hmr_raf_" + placeholderId) <- null

/// Create a fresh container with id, append it to the body so React can find it.
let private createContainer (placeholderId: string) =
    // Remove existing element if any
    let old = document.getElementById placeholderId
    if not (isNull old) then
        old.parentElement.removeChild old |> ignore
    let container = document.createElement "div"
    container.id <- placeholderId
    document.body.appendChild container |> ignore
    container

/// Create a JS component that tracks mount count via useEffect.
let private createTrackedComponent () =
    let counter : obj = emitJsExpr () "({current: 0})"
    let getMountCount () : int = emitJsExpr counter "$0.current"
    let react : obj = emitJsExpr ReactBindings.React "$0"
    let comp : obj =
        emitJsExpr
            (react, counter)
            """(function() {
                var R = $0, ctr = $1;
                function TrackedComp(props) {
                    R.useEffect(function() { ctr.current++; }, []);
                    return R.createElement('span', null, String(props.model));
                }
                return TrackedComp;
            })()"""
    comp, getMountCount

let tests = testList "Program.withReact* (HMR root caching)" [

    // -------------------------------------------------------
    // RootCache.getOrCreateRoot
    // -------------------------------------------------------

    testList "RootCache.getOrCreateRoot" [

        testCase "returns same root for same placeholderId" <| fun _ ->
            let id = "test-root-cache-1"
            let _container = createContainer id
            clearCachedRoot id

            let root1 = Program.RootCache.getOrCreateRoot id
            let root2 = Program.RootCache.getOrCreateRoot id

            Expect.isTrue
                (obj.ReferenceEquals(root1, root2))
                "should return the same cached root instance"
            clearCachedRoot id

        testCase "returns different roots for different placeholderIds" <| fun _ ->
            let id1 = "test-root-cache-2a"
            let id2 = "test-root-cache-2b"
            let _c1 = createContainer id1
            let _c2 = createContainer id2
            clearCachedRoot id1
            clearCachedRoot id2

            let root1 = Program.RootCache.getOrCreateRoot id1
            let root2 = Program.RootCache.getOrCreateRoot id2

            Expect.isFalse
                (obj.ReferenceEquals(root1, root2))
                "different placeholders should get different roots"
            clearCachedRoot id1
            clearCachedRoot id2
    ]

    // -------------------------------------------------------
    // No-remount on repeated renders through cached root
    // -------------------------------------------------------

    testList "no remount on state updates" [

        testCase "component is not remounted when model changes via cached root" <| fun _ ->
            resetHmrCount 0
            let id = "test-no-remount-1"
            let _container = createContainer id
            clearCachedRoot id

            let (comp, getMountCount) = createTrackedComponent ()

            let view (model: int) (_dispatch: unit Dispatch) : ReactElement =
                ReactBindings.React.createElement(unbox comp, {| model = model |}, [])

            let render = lazyView2With (fun x y -> obj.ReferenceEquals(x, y)) view
            let root = Program.RootCache.getOrCreateRoot id

            // First render — component mounts
            flushSync (fun () -> root.render (render 1 ignore))
            Expect.equal (getMountCount()) 1 "initial mount"

            // Second render with different model — component updates, not remounted
            flushSync (fun () -> root.render (render 2 ignore))
            Expect.equal (getMountCount()) 1 "update should not remount"

            // Third render with yet another model
            flushSync (fun () -> root.render (render 3 ignore))
            Expect.equal (getMountCount()) 1 "still no remount on further updates"
            clearCachedRoot id

        testCase "simulating HMR reload: reusing cached root does not remount" <| fun _ ->
            resetHmrCount 0
            let id = "test-hmr-sim-1"
            let _container = createContainer id
            clearCachedRoot id

            let (comp, getMountCount) = createTrackedComponent ()

            let view (model: int) (_dispatch: unit Dispatch) : ReactElement =
                ReactBindings.React.createElement(unbox comp, {| model = model |}, [])

            // Simulate first module load
            let render1 = lazyView2With (fun x y -> obj.ReferenceEquals(x, y)) view
            let root1 = Program.RootCache.getOrCreateRoot id

            flushSync (fun () -> root1.render (render1 1 ignore))
            Expect.equal (getMountCount()) 1 "initial mount"

            // Simulate HMR reload — same view function reference but new lazyView2With call
            // and getOrCreateRoot returns the CACHED root
            resetHmrCount 1
            let render2 = lazyView2With (fun x y -> obj.ReferenceEquals(x, y)) view
            let root2 = Program.RootCache.getOrCreateRoot id

            Expect.isTrue
                (obj.ReferenceEquals(root1, root2))
                "root should be cached across simulated HMR reload"

            // The render function from the same `view` reference shares the __memo component
            flushSync (fun () -> root2.render (render2 1 ignore))
            Expect.equal (getMountCount()) 1 "same view ref + cached root should not remount"
            clearCachedRoot id

        testCase "withReactSynchronous does not remount on multiple setState calls" <| fun _ ->
            resetHmrCount 0
            let id = "test-sync-no-remount"
            let _container = createContainer id
            clearCachedRoot id

            let (comp, getMountCount) = createTrackedComponent ()

            let view (model: int) (_dispatch: unit Dispatch) : ReactElement =
                ReactBindings.React.createElement(unbox comp, {| model = model |}, [])

            let render = lazyView2With (fun x y -> obj.ReferenceEquals(x, y)) view
            let root = Program.RootCache.getOrCreateRoot id

            flushSync (fun () -> root.render (render 1 ignore))
            Expect.equal (getMountCount()) 1 "initial mount"

            flushSync (fun () -> root.render (render 2 ignore))
            Expect.equal (getMountCount()) 1 "sync update should not remount"
            clearCachedRoot id
    ]

    // -------------------------------------------------------
    // RAF cancellation
    // -------------------------------------------------------

    testList "pending RAF cancellation" [

        testCase "savePendingRaf and cancelPendingRaf round-trip" <| fun _ ->
            let id = "test-raf-cancel"
            clearCachedRoot id

            // Store a dummy RAF handle
            Program.RootCache.savePendingRaf id 12345.0

            let stored: float option = unbox window?("__elmish_hmr_raf_" + id)
            Expect.equal stored (Some 12345.0) "RAF handle should be stored"

            // Cancel it
            Program.RootCache.cancelPendingRaf id

            let afterCancel: obj = window?("__elmish_hmr_raf_" + id)
            Expect.isTrue (isNull afterCancel) "RAF handle should be cleared after cancel"
    ]
]
