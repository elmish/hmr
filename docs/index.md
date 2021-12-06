---
layout: standard
toc: false
---

## Hot Module Replacement

Elmish applications can benefit from Hot Module Replacement (known as HMR).

This allow us to modify the application while it's running, without a full reload. Your application will now maintain its state between two changes.

![hmr demo](/hmr/static/img/hmr_demo.gif)

## Installation
Add Fable package with paket:

```sh
paket add nuget Fable.Elmish.HMR
```

## Webpack configuration

Add `hot: true` and `inline: true` to your `devServer` node.

Example:

```js
// ...
devServer: {
    // ...
    hot: true
}
// ...
```

## Parcel and Vite

Parcel and Vite, are supported since version 4.2.0. They don't require any specific configuration.

## Usage

The package will include the HMR support only if you are building your program with `DEBUG` set in your compilation conditions. Fable adds it by default when in watch mode.

You need to always include `open Elmish.HMR` after your others `open Elmish.XXX` statements. This is needed to shadow the supported APIs.

For example, if you use `Elmish.Program.run` it will be shadowed as `Elmish.HMR.Program.run`.

```fs
open Elmish
open Elmish.React
open Elmish.HMR // See how this is the last open statement

Program.mkProgram init update view
|> Program.withReactSynchronous "elmish-app"
|> Program.run
```

You can also use `Elmish.Program.runWith` if you need to pass custom arguments, `runWith` will also be shadowed as `Elmish.HMR.Program.runWith`:

```fs
Program.mkProgram init update view
|> Program.withReactSynchronous "elmish-app"
|> Program.runWith ("custom argument", 42)
```
