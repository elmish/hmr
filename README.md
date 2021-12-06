Elmish-HMR: [Hot Module Replacement](https://webpack.js.org/concepts/hot-module-replacement/) integration for [elmish](https://github.com/fable-compiler/elmish) applications.
=======

[![NuGet version](https://badge.fury.io/nu/Fable.Elmish.HMR.svg)](https://badge.fury.io/nu/Fable.Elmish.HMR)

For more information see [the docs](https://elmish.github.io/hmr).

## Installation

```sh
paket add nuget Fable.Elmish.HMR
```

## Demo

![hmr demo](https://raw.githubusercontent.com/elmish/hmr/master/docs/static/img/hmr_demo.gif)

## Development

This repository use NPM scripts to control the build system here is a list of the main scripts available:

| Script | Description |
|---|---|
| `npm run tests:watch` | To use when working on the tests suits |
| `npm run docs:watch` | To use when working on the documentation, hosted on [http://localhost:8080](http://localhost:8080) |
| `npm run docs:publish` | Build a new version of the documentation and publish it to Github Pages |
| `npm run release` | Build a new version of the packages if needed and release it |
