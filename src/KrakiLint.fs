module KrakiLint

open Util.Option
open Util
open NJsonSchema
open FSharp.Json

let private validateSchema schemaTemplate value =
    match Util.isString schemaTemplate, Util.isString value with
    | true, true -> Ok ()
    | true, _ -> Error (KrakiError.schemaError schemaTemplate value)
    | _, true -> Error (KrakiError.schemaError schemaTemplate value)
    | _, _ ->
        let schema = JsonSchema.FromSampleJson(Json.serialize(schemaTemplate))
        match Json.serialize(value) |> schema.Validate |> Seq.length with
        | 0 -> Ok ()
        | _ -> Error (KrakiError.schemaError schemaTemplate value)

let private checkEndpointFields fields endpoint =
    Map.fold(fun errors field schema ->
        match Map.tryFind field endpoint with
        | Some value ->
            match validateSchema schema value with
            | Ok () -> errors
            | Error moreErrors -> moreErrors :: errors
        | None -> KrakiError.missingRequiredField field :: errors
    ) [] fields

let private checkRequiredFields (fields : Map<string, obj>) (krakendEndpoints : array<Endpoint.Endpoint>) =
    Array.fold (fun report endpoint ->
        match checkEndpointFields fields endpoint with
        | [] -> report
        | errors -> Report.extend (Endpoint.toSafeId endpoint) errors report
    ) Report.empty krakendEndpoints

let private makeSortProjection sortKeys =
    fun endpoint -> List.map (fun key -> string (Map.find key endpoint)) sortKeys

let private makeSortID sortKeys endpoint =
    makeSortProjection sortKeys endpoint |> String.concat ""

let private maybeExtendSortReport keys krakend sorted report = 
    match makeSortID keys sorted, makeSortID keys krakend with
    | s, k when s > k ->
        Report.extend
        <| Endpoint.toId krakend
        <| [KrakiError.incorrectSortOrder <| Endpoint.toId sorted]
        <| report
    | _ -> report

let private checkSorting (sortCfg : Kraki.SortConfig) (krakendEndpoints : array<Endpoint.Endpoint>) =
    match Kraki.checkKeysHaveStringValue sortCfg.by krakendEndpoints |> Report.toOption with
    | Some report -> report
    | None ->
        Seq.fold2 (fun report' krakendEndpoint sortedEndpoint ->
            match krakendEndpoint = sortedEndpoint with
            | true -> report'
            | false -> maybeExtendSortReport sortCfg.by krakendEndpoint sortedEndpoint report'
        ) Report.empty krakendEndpoints <| Seq.sortBy (makeSortProjection sortCfg.by) krakendEndpoints

let private checkEndpoints (krakendEndpoints : array<Endpoint.Endpoint>) (endpointCfg : Kraki.EndpointConfig) =
    [ checkRequiredFields <!> endpointCfg.require;
      checkSorting <!> endpointCfg.sort]
    |> Option.catOptions
    |> List.map (fun f -> f krakendEndpoints)
    |> Report.merge

let lint (krakiCfg : Kraki.KrakiConfig) (krakendCfg : Krakend.KrakendConfig) =
    let endpoints = krakiCfg.lint >>= fun linterCfg -> linterCfg.endpoints
    match checkEndpoints <!> krakendCfg.endpoints <*> endpoints >>= Report.toOption with
    | Some report -> Error report
    | None -> Ok ()
