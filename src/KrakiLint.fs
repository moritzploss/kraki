module KrakiLint

open Util.Option
open Util

let private validateSchema (schema : Kraki.Schema) (krakendEndpoints : list<Endpoint.Endpoint>) =
    Kraki.checkSchema schema krakendEndpoints Report.empty

let private makeSortProjection sortKeys =
    fun endpoint -> List.map (fun key -> string (Map.find key endpoint)) sortKeys

let private makeSortID sortKeys endpoint =
    makeSortProjection sortKeys endpoint |> String.concat ""

let private maybeExtendSortReport keys krakend sorted report = 
    match makeSortID keys sorted, makeSortID keys krakend with
    | s, k when s > k ->
        Report.extend
        <| Endpoint.toId krakend
        <| [KrakiError.incorrectSortOrder "Endpoint" <| Endpoint.toId sorted]
        <| report
    | _ -> report

let private validateSorting (sortCfg : Kraki.SortConfig) (krakendEndpoints : list<Endpoint.Endpoint>) =
    match Kraki.checkKeysExist sortCfg.by krakendEndpoints Report.empty |> Report.toOption with
    | Some report -> report
    | None ->
        Seq.fold2 (fun report' krakendEndpoint sortedEndpoint ->
            match krakendEndpoint = sortedEndpoint with
            | true -> report'
            | false -> maybeExtendSortReport sortCfg.by krakendEndpoint sortedEndpoint report'
        ) Report.empty krakendEndpoints <| Seq.sortBy (makeSortProjection sortCfg.by) krakendEndpoints

let private checkEndpoints (krakendEndpoints : list<Endpoint.Endpoint>) (endpointCfg : Kraki.EndpointConfig) =
    match validateSchema <!> endpointCfg.schema <*> Some krakendEndpoints >>= Report.toOption with
    | Some report -> Some report
    | None -> validateSorting <!> endpointCfg.sort <*> Some krakendEndpoints >>= Report.toOption

let lint (krakiCfg : Kraki.KrakiConfig) (krakendCfg : Krakend.KrakendConfig) =
    let endpoints = krakiCfg.lint >>= fun linterCfg -> linterCfg.endpoints
    match checkEndpoints <!> krakendCfg.endpoints <*> endpoints |> Option.flatten with
    | Some report -> Error report
    | None -> Ok ()
