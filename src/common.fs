namespace Elmish.HMR

open Fable.Core.JsInterop
open Fable.React
open Browser
open Elmish

[<AutoOpen>]
module Common =

#if DEBUG
    [<NoComparison; NoEquality>]
    type private HmrMemoProps1<'model> = {
        model: 'model
        equal: 'model -> 'model -> bool
        hmrCount: int
    }

    [<NoComparison; NoEquality>]
    type private HmrMemoProps2<'model,'msg> = {
        model: 'model
        dispatch: 'msg Dispatch
        equal: 'model -> 'model -> bool
        hmrCount: int
    }

    let private getHmrCount () =
        if isNull window?Elmish_HMR_Count then 0
        else unbox<int> window?Elmish_HMR_Count

    /// Avoid rendering the view unless the model has changed.
    /// equal: function to compare the previous and the new states
    /// view: function to render the model
    /// state: new state to render
    let lazyViewWith (equal:'model->'model->bool)
                     (view:'model->ReactElement)
                     (state:'model) =
        let memoized : ReactElementType<HmrMemoProps1<'model>> =
            emitJsExpr
                (view, fun () ->
                    let m =
                        ReactBindings.React.memo(
                            (fun (props: HmrMemoProps1<'model>) -> view props.model),
                            (fun (prev: HmrMemoProps1<'model>) (next: HmrMemoProps1<'model>) ->
                                prev.hmrCount = next.hmrCount
                                && next.equal prev.model next.model))
                    emitJsStatement (m, view) "$0.displayName = $1.name || void 0"
                    m)
                "$0.__memo || ($0.__memo = $1())"
        ReactBindings.React.createElement(memoized, { model = state; equal = equal; hmrCount = getHmrCount() }, [])

    /// Avoid rendering the view unless the model has changed.
    /// equal: function to compare the previous and the new states
    /// view: function to render the model using the dispatch
    /// Partially apply with equal and view to get a cached rendering function:
    /// let render = lazyView2With equal view
    /// render state dispatch
    let lazyView2With (equal:'model->'model->bool)
                      (view:'model->'msg Dispatch->ReactElement) =
        let memoized : ReactElementType<HmrMemoProps2<'model,'msg>> =
            emitJsExpr
                (view, fun () ->
                    let m =
                        ReactBindings.React.memo(
                            (fun (props: HmrMemoProps2<'model,'msg>) -> view props.model props.dispatch),
                            (fun (prev: HmrMemoProps2<'model,'msg>) (next: HmrMemoProps2<'model,'msg>) ->
                                prev.hmrCount = next.hmrCount
                                && next.equal prev.model next.model))
                    emitJsStatement (m, view) "$0.displayName = $1.name || void 0"
                    m)
                "$0.__memo || ($0.__memo = $1())"
        fun (state:'model) (dispatch:'msg Dispatch) ->
            ReactBindings.React.createElement(memoized, { model = state; dispatch = dispatch; equal = equal; hmrCount = getHmrCount() }, [])

    /// Avoid rendering the view unless the model has changed.
    /// equal: function to compare the previous and the new model (a tuple of two states)
    /// view: function to render the model using the dispatch
    /// Partially apply with equal and view to get a cached rendering function:
    /// let render = lazyView3With equal view
    /// render state1 state2 dispatch
    let lazyView3With (equal:_->_->bool) (view:_->_->_->ReactElement) =
        let memoized : ReactElementType<HmrMemoProps2<_,_>> =
            emitJsExpr
                (view, fun () ->
                    let m =
                        ReactBindings.React.memo(
                            (fun (props: HmrMemoProps2<_,_>) ->
                                let (s1, s2) = props.model
                                view s1 s2 props.dispatch),
                            (fun (prev: HmrMemoProps2<_,_>) (next: HmrMemoProps2<_,_>) ->
                                prev.hmrCount = next.hmrCount
                                && next.equal prev.model next.model))
                    emitJsStatement (m, view) "$0.displayName = $1.name || void 0"
                    m)
                "$0.__memo || ($0.__memo = $1())"
        fun state1 state2 (dispatch:'msg Dispatch) ->
            ReactBindings.React.createElement(memoized, { model = (state1, state2); dispatch = dispatch; equal = equal; hmrCount = getHmrCount() }, [])

    /// Avoid rendering the view unless the model has changed.
    /// view: function of model to render the view
    let inline lazyView (view:'model->ReactElement) =
        lazyViewWith (=) view

    /// Avoid rendering the view unless the model has changed.
    /// view: function of two arguments to render the model using the dispatch
    let lazyView2 (view:'model->'msg Dispatch->ReactElement) =
        lazyView2With (=) view

    /// Avoid rendering the view unless the model has changed.
    /// view: function of three arguments to render the model using the dispatch
    let lazyView3 (view:_->_->_->ReactElement) =
        lazyView3With (=) view

#else
    /// Avoid rendering the view unless the model has changed.
    /// equal: function to compare the previous and the new states
    /// view: function to render the model
    /// state: new state to render
    let inline lazyViewWith (equal:'model->'model->bool)
                     (view:'model->ReactElement)
                     (state:'model) =
        Elmish.React.Common.lazyViewWith equal view state

    /// Avoid rendering the view unless the model has changed.
    /// equal: function to compare the previous and the new states
    /// view: function to render the model using the dispatch
    /// Partially apply with equal and view to get a cached rendering function
    let inline lazyView2With (equal:'model->'model->bool)
                      (view:'model->'msg Dispatch->ReactElement) =
        Elmish.React.Common.lazyView2With equal view

    /// Avoid rendering the view unless the model has changed.
    /// equal: function to compare the previous and the new model (a tuple of two states)
    /// view: function to render the model using the dispatch
    /// Partially apply with equal and view to get a cached rendering function
    let inline lazyView3With (equal:_->_->bool) (view:_->_->_->ReactElement) =
        Elmish.React.Common.lazyView3With equal view

    /// Avoid rendering the view unless the model has changed.
    /// view: function of model to render the view
    let inline lazyView (view:'model->ReactElement) =
        Elmish.React.Common.lazyView view

    /// Avoid rendering the view unless the model has changed.
    /// view: function of two arguments to render the model using the dispatch
    let inline lazyView2 (view:'model->'msg Dispatch->ReactElement) =
        Elmish.React.Common.lazyView2 view

    /// Avoid rendering the view unless the model has changed.
    /// view: function of three arguments to render the model using the dispatch
    let inline lazyView3 (view:_->_->_->ReactElement) =
        Elmish.React.Common.lazyView3 view
#endif
