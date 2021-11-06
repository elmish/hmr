[<RequireQualifiedAccess>]
module Router

open Browser
open Elmish.Navigation
open Elmish.UrlParser
open Feliz

type Route =
    | Home
    | About

let private segment (pathA : string) (pathB : string) =
    pathA + "/" + pathB

let private toHash page =
    match page with
    | Home -> "home"
    | About -> "about"

    |> segment "#"

let pageParser : Parser<Route -> Route, Route> =
    oneOf
        [
            map Home (s "home")
            map About (s "about")

            map Home top
        ]


let href route =
    prop.href (toHash route)

let modifyUrl route =
    route |> toHash |> Navigation.modifyUrl

let newUrl route =
    route |> toHash |> Navigation.newUrl

let modifyLocation route =
    window.location.href <- toHash route
