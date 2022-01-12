module Endpoint

let getIdentifiers endpoint =
    let method = Map.find "method" endpoint |> string
    let path = Map.find "endpoint" endpoint |> string
    (method, path)

let toPaddedId endpoint =
    let (method, path) = getIdentifiers endpoint
    $"{method.PadRight(7)} {path}"

let toId endpoint =
    let (method, path) = getIdentifiers endpoint
    $"{method} {path}"

let toSafeId endpoint =
    try
        toId endpoint
    with
    | _ -> "<< unknown endpoint >>"
