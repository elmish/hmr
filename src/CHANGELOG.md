# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## Unreleased

## 8.0.0 - 2025-07-20

* Update to support Fable.React 9.4 (by @bigjonroberts)
* Update to support Elmish 5 (by @bigjonroberts)

## 7.0.0 - 2022-12-22

* Breaking: support for v4 elmish

## 6.0.0-beta-003 - 2022-11-09

* Support newest version of Elmish v4 with the new subscription system (by @kspeakman)

## 6.0.0-beta-002 - 2021-11-30

* Remove Fable.Elmish.Browser dependency
* Fix HMR support, it was not working in beta-001

## 6.0.0-beta-001 - 2021-11-29

* Support for elmish v4
* Remove `Program.toNavigable` shadowing because `Elmish.Browser` can now remove its listeners by itself

## 6.0.0 - 2021-11-29

* UNLISTED
* Support for elmish v4
* Remove `Program.toNavigable` shadowing because `Elmish.Browser` can now remove its listeners by itself

## 5.2.0 - 2021-11-30

* PR #37: Mitigate problem where `hot.dispose` run late compared to `hot.accept` (Vite seems to be doing that sometimes...)

## 5.1.0 - 2021-11-26

* PR #36: Add a comment on Webpack HMR support detection to redirect the user to the issue explaining what is happening.

## 5.0.0 - 2021-11-25

* Make a new major released which include un-listed version 4.2.0, 4.2.1, 4.3.0, and 4.3.1
* Add a comment on Vite HMR support detection to redirect the user to the issue explaining what is happening.
* Fix `Program.toNavigable` version when a Bundler is detected. It was not registering the listener for the location change.

## 4.3.1 - 2021-11-15

* Fix #34: Lower FSharp.Core requirement

## 4.3.0 - 2021-11-09

* Rework bundler detection adding `try ... with` when detecting the bundler used to avoid `RefrenceError` exception

## 4.2.1 - 2021-11-08

* Fix GIF url in the README

## 4.2.0 - 2021-11-08

* Fix #29: Add support for Webpack, Parcel and Vite

## 4.1.0

* Upgrade dependency Fable.Elmish.Browser to 3.0.4 (see https://github.com/elmish/hmr/issues/27)

## 4.0.1

* Fix #25: Fix React shadowing calls to use the `LazyView` from `HMR`

## 4.0.0

* Release stable version

## 4.0.0-beta-6

* Upgrade Fable.Elmish.Browser (by @alfonsogarciacaro)
* Re-add the non debuggin Helpers so people can just `open Elmish.HMR` (by @alfonsogarciacaro)

## 4.0.0-beta-5

* Inline functions (by @alfonsogarciacaro)

## 4.0.0-beta-4

* Update to Elmish 3.0.0-beta-7 (by @alfonsogarciacaro)

## 4.0.0-beta-3

* Update to `Fable.Core` v3-beta

## 4.0.0-beta-2

* Includes: Fix ReactNative
* Includes: Add shadowing function for `runWith` by @theimowski

## 4.0.0-beta-1

* Elmish 3.0 compat

## 3.2.0

* Fix ReactNative

## 3.1.0

* Add shadowing function for `runWith` by @theimowski

## 3.0.2

* Fix HMR by including `Elmish_HMR_Count` variable in window namespace

## 3.0.1

* Fix `Release` version of `lazyView`, `lazyView2`, `lazyView3` and `lazyViewWith`

## 3.0.0

* Fix HMR support for Elmish apps and shadow more API

## 2.1.0

* Release stable

## 2.1.0-beta-1

* Add shadow version of `Program.toNavigable`

## 2.0.0

* Release 2.0.0
* Use Fable.Import.HMR instead of local binding

## 2.0.0-beta-4

* Re-releasing v1.x for Fable 2

## 1.0.1

* Backporting build

## 1.0.0

* Release 1.0.0

## 1.0.0-beta-1

* Release in beta

## 0.1.0-alpha.1

* Initial hmr release
