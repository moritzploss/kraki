module KrakiLint

open Util.Option
open Util

let private validateSchema (schema : Kraki.Schema) (endpoints : list<Endpoint.Endpoint>) =
    Endpoint.validate (Kraki.validateSchema schema) endpoints

let private validateKeysExist (keys: list<string>) (endpoints : list<Endpoint.Endpoint>) =
    Endpoint.validate (Kraki.validateKeysExist keys) endpoints

let private addSortError expected actual report =
    Report.extend (Endpoint.toId actual) [KrakiMessage.wrongSortOrder (Endpoint.toId expected)] report

let private validateSortOrder (sortKeys: list<string>) (endpoints : list<Endpoint.Endpoint>) =
    let projection endpoint =
        List.map (fun key -> (string (Map.find key endpoint)).ToLower()) sortKeys
    let offender =
        Seq.zip (Seq.ofList endpoints) (Seq.sortBy projection endpoints)
        |> fun seq -> lazy Seq.find (fun (actual, expected) -> not (expected = actual)) seq
        |> Option.ofFailable
    match offender with
    | None -> Report.empty
    | Some (actual, expected) -> addSortError expected actual Report.empty

let private validateSorting (sortCfg : Kraki.SortConfig) (endpoints : list<Endpoint.Endpoint>) =
    match validateKeysExist sortCfg.by endpoints |> Report.toOption with
    | Some report -> report
    | None -> validateSortOrder sortCfg.by endpoints

let private checkEndpoints (krakendEndpoints : list<Endpoint.Endpoint>) (endpointCfg : Kraki.EndpointConfig) =
    match validateSchema <!> endpointCfg.schema <*> Some krakendEndpoints >>= Report.toOption with
    | Some report -> Some report
    | None -> validateSorting <!> endpointCfg.sort <*> Some krakendEndpoints >>= Report.toOption

let lint (krakiCfg : Kraki.KrakiConfig) (krakendCfg : Krakend.KrakendConfig) =
    let endpoints = krakiCfg.lint >>= fun linterCfg -> linterCfg.endpoints
    match checkEndpoints <!> krakendCfg.endpoints <*> endpoints |> Option.flatten with
    | Some report -> Error report
    | None -> Ok ()
