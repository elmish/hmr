namespace Elmish.HMR

open Fable.Import.React
open Elmish

[<AutoOpen>]
module Common =

    #if DEBUG
    /// Avoid rendering the view unless the model has changed.
    /// equal: function to compare the previous and the new states
    /// view: function to render the model
    /// state: new state to render
    let lazyViewWith (equal:'model->'model->bool)
                     (view:'model->ReactElement)
                     (state:'model) =
        view state
    #else
    /// Avoid rendering the view unless the model has changed.
    /// equal: function to compare the previous and the new states
    /// view: function to render the model
    /// state: new state to render
    let inline lazyViewWith (equal:'model->'model->bool)
                     (view:'model->ReactElement)
                     (state:'model) =
        Elmish.React.Common.lazyViewWith
    #endif

    #if DEBUG
    /// Avoid rendering the view unless the model has changed.
    /// equal: function to compare the previous and the new states
    /// view: function to render the model using the dispatch
    /// state: new state to render
    /// dispatch: dispatch function
    let lazyView2With (equal:'model->'model->bool)
                      (view:'model->'msg Dispatch->ReactElement)
                      (state:'model)
                      (dispatch:'msg Dispatch) =
        view state dispatch
    #else
    /// Avoid rendering the view unless the model has changed.
    /// equal: function to compare the previous and the new states
    /// view: function to render the model using the dispatch
    /// state: new state to render
    /// dispatch: dispatch function
    let inline lazyView2With (equal:'model->'model->bool)
                      (view:'model->'msg Dispatch->ReactElement)
                      (state:'model)
                      (dispatch:'msg Dispatch) =
        Elmish.React.Common.lazyView2With equal view state dispatch
    #endif

    #if DEBUG
    /// Avoid rendering the view unless the model has changed.
    /// equal: function to compare the previous and the new model (a tuple of two states)
    /// view: function to render the model using the dispatch
    /// state1: new state to render
    /// state2: new state to render
    /// dispatch: dispatch function
    let lazyView3With (equal:_->_->bool) (view:_->_->_->ReactElement) state1 state2 (dispatch:'msg Dispatch) =
        view state1 state2 dispatch
    #else
    /// Avoid rendering the view unless the model has changed.
    /// equal: function to compare the previous and the new model (a tuple of two states)
    /// view: function to render the model using the dispatch
    /// state1: new state to render
    /// state2: new state to render
    /// dispatch: dispatch function
    let inline lazyView3With (equal:_->_->bool) (view:_->_->_->ReactElement) state1 state2 (dispatch:'msg Dispatch) =
        Elmish.React.Common.lazyView3With equal view state1 state2 dispatch
    #endif


    #if DEBUG
    /// Avoid rendering the view unless the model has changed.
    /// view: function of model to render the view
    let lazyView (view:'model->ReactElement) =
        view
    #else
    /// Avoid rendering the view unless the model has changed.
    /// view: function of model to render the view
    let inline lazyView (view:'model->ReactElement) =
        Elmish.React.Common.lazyView
    #endif

    #if DEBUG
    /// Avoid rendering the view unless the model has changed.
    /// view: function of two arguments to render the model using the dispatch
    let lazyView2 (view:'model->'msg Dispatch->ReactElement) =
        view
    #else
    /// Avoid rendering the view unless the model has changed.
    /// view: function of two arguments to render the model using the dispatch
    let inline lazyView2 (view:'model->'msg Dispatch->ReactElement) =
        Elmish.React.Common.lazyView2
    #endif

    #if DEBUG
    /// Avoid rendering the view unless the model has changed.
    /// view: function of three arguments to render the model using the dispatch
    let lazyView3 (view:_->_->_->ReactElement) =
        view
    #else
    /// Avoid rendering the view unless the model has changed.
    /// view: function of three arguments to render the model using the dispatch
    let inline lazyView3 (view:_->_->_->ReactElement) =
        Elmish.React.Common.lazyView3
    #endif
