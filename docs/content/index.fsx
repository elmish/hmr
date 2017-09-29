(*** hide ***)
#I "../../src/bin/Debug/netstandard1.6"
#I "../../packages/Fable.Elmish/lib/netstandard1.6"
#r "Fable.Elmish.dll"
#r "Fable.Elmish.HMR.dll"
open Elmish

(** Hot Module Replacement
======================
Elmish applications can benefit from Hot Module Replacement (known as HMR).

This allow us to modify the application while it's running, without a full reload. Your application will now maintain its state between two changes.

![hmr demo](https://fable-elmish.github.io/hmr/img/hmr_demo.gif)


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

You also need to add this two plugins when building in development mode:

- `webpack.HotModuleReplacementPlugin`
- `webpack.NamedModulesPlugin`

Example:
```js
plugins : isProduction ? [] : [
    new webpack.HotModuleReplacementPlugin(),
    new webpack.NamedModulesPlugin()
]
```

You can find a complete `webpack.config.js` [here](https://github.com/fable-elmish/templates/blob/master/src/react-demo/Content/webpack.config.js).

### Program module functions

Augment your program instance with HMR support.

*IMPORTANT*: Make sure to add HMR support before `Program.withReact` or `Program.withReactNative` line.

Usage:
*)


open Elmish.HMR

Program.mkProgram init update view
|> Program.withHMR // Add the HMR support
|> Program.withReact "elmish-app"
|> Program.run

(**

and if you use React Native:
*)


open Elmish.HMR

Program.mkProgram init update view
|> Program.withHMR // Add the HMR support
|> Program.withReactNative "elmish-app"
|> Program.run

(**
### Conditional compilation
If don't want to include the debugger in production builds surround it with `#if DEBUG`/`#endif` and define the symbol conditionally in your build system.

*)
