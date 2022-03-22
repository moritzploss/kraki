module Endpoint

type Endpoint = Map<string, obj>

let private find key def map  =
    match Map.tryFind key map with
    | Some value -> string value
    | None -> def

let getIdentifiers (endpoint : Endpoint) =
    let method = find "method" "GET" endpoint
    let path = find "endpoint" "<unknown>" endpoint
    (method, path)

let toPaddedId (endpoint : Endpoint) =
    let (method, path) = getIdentifiers endpoint
    $"{method.PadRight(7)} {path}"

let toId (endpoint : Endpoint) =
    let (method, path) = getIdentifiers endpoint
    $"{method} {path}"

let validate validator endpoints =
    List.fold (fun report endpoint ->
        match validator endpoint with
        | [] -> report
        | errors -> Report.extend (toId endpoint) errors report
    ) Report.empty endpoints
