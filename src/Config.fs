module Config

open FSharp.Json
open Util

let private parse<'T> lax jsonString =
    let config = JsonConfig.create(allowUntyped = lax)
    Result.ofFailable <| lazy Json.deserializeEx<'T> config jsonString

let parseStrict<'T> jsonString =
    parse<'T> false jsonString

let parseLax<'T> jsonString =
    parse<'T> true jsonString
