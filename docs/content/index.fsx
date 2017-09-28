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

![hmr demo](https://github.com/zalmoxisus/remotedev/raw/master/docs/files/img/hmr_demo.gif)


### Installation
Add Fable package with paket:

```shell
paket add nuget Fable.Elmish.HMR
```

### Program module functions
Augment your program instance with HMR support.

If you are using Elmish with react, make sure to add the HMR support before `withReact` line.

Usage:
*)


open Elmish.HMR

Program.mkProgram init update view
|> Program.withHMR // Add the HMR support
|> Program.withReact "elmsih-app"
|> Program.run

(**

### Conditional compilation
If don't want to include the debugger in production builds surround it with `#if DEBUG`/`#endif` and define the symbol conditionally in your build system.

*)
