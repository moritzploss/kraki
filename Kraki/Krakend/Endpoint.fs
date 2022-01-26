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
    | _ -> "unknown endpoint (missing key 'endpoint' or 'method')"

let validate validator endpoints =
    List.fold (fun report endpoint ->
        match validator endpoint with
        | [] -> report
        | errors -> Report.extend (toId endpoint) errors report
    ) Report.empty endpoints
