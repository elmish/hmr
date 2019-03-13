(*** hide ***)
#I "../../src/bin/Debug/netstandard1.6"
#I "../../.paket/load/netstandard1.6"
#r "Fable.Elmish.dll"
#r "Fable.Elmish.HMR.dll"
open Elmish

(**
Hot Module Replacement
======================
Elmish applications can benefit from Hot Module Replacement (known as HMR).

This allow us to modify the application while it's running, without a full reload. Your application will now maintain its state between two changes.

![hmr demo](https://elmish.github.io/hmr/img/hmr_demo.gif)


### Installation
Add Fable package with paket:

```shell
paket add nuget Fable.Elmish.HMR
```

### Webpack configuration

Add `hot: true` and `inline: true` to your `devServer` node.

Example:
```js
devServer: {
    contentBase: resolve('./public'),
    port: 8080,
    hot: true,
    inline: true
}
```

You also need to add `webpack.HotModuleReplacementPlugin` when building in development mode:

Example:
```js
plugins : isProduction ? [] : [
    new webpack.HotModuleReplacementPlugin()
]
```

Note: second plugin that's necessary, `new webpack.NamedModulesPlugin()` is added automatically by Webpack in development mode: [https://webpack.js.org/concepts/mode/](https://webpack.js.org/concepts/mode/)

You can find a complete `webpack.config.js` [here](https://github.com/elmish/templates/blob/master/src/react-demo/Content/webpack.config.js).

### Usage

The package will include the HMR support only if you are building your program with `DEBUG` set in your compilation conditions.

You need to always include `open Elmish.HMR` after your others `open Elmish.XXX` statements. This is needed to shadow supported APIs.

For example, if you use `Elmish.Program.run` it will be shadowed as `Elmish.HMR.Program.run`.

*)

open Elmish
open Elmish.React
open Elmish.HMR // See how this is the last open statement

Program.mkProgram init update view
|> Program.withReactSynchronous "elmish-app"
|> Program.run

(**

You can also use `Elmish.Program.runWith` if you need to pass custom arguments, `runWith` will also be shadowed as `Elmish.HMR.Program.runWith`:

*)

Program.mkProgram init update view
|> Program.withReactSynchronous "elmish-app"
|> Program.runWith ("custom argument", 42)