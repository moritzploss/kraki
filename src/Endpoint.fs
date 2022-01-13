module Endpoint

type Endpoint = Map<string, obj>

let getIdentifiers (endpoint : Endpoint) =
    let method = Map.find "method" endpoint |> string
    let path = Map.find "endpoint" endpoint |> string
    (method, path)

let toPaddedId (endpoint : Endpoint) =
    let (method, path) = getIdentifiers endpoint
    $"{method.PadRight(7)} {path}"

let toId (endpoint : Endpoint) =
    let (method, path) = getIdentifiers endpoint
    $"{method} {path}"

let toSafeId (endpoint : Endpoint) =
    try
        toId endpoint
    with
    | _ -> "<< unknown endpoint >>"
