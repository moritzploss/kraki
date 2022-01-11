module KrakiLint

open Util.Option
open Util
open NJsonSchema
open FSharp.Json
open Kraki

let makeSchemaError schemaTemplate value =
    Error $"Value '{value}' does not match schema template '{schemaTemplate}'"

let validateSchema schemaTemplate value =
    match Util.isString schemaTemplate, Util.isString value with
    | true, true -> Ok ()
    | true, _ -> makeSchemaError schemaTemplate value
    | _, true -> makeSchemaError schemaTemplate value
    | _, _ ->
        let schema = JsonSchema.FromSampleJson(Json.serialize(schemaTemplate))
        match Json.serialize(value) |> schema.Validate |> Seq.length with
        | 0 -> Ok ()
        | _ -> makeSchemaError schemaTemplate value

let checkEndpointFields fields endpoint =
    Map.fold(fun errors field schema ->
        match Map.tryFind field endpoint with
        | Some value ->
            match validateSchema schema value with
            | Ok () -> errors
            | Error moreErrors -> moreErrors :: errors
        | None -> $"Missing key '{field}'" :: errors
    ) [] fields

let checkFields (krakendEndpoints : array<Krakend.Endpoint>) (fields : Map<string, obj>) =
    Array.fold (fun report endpoint ->
        match checkEndpointFields fields endpoint with
        | [] -> report
        | errors -> Map.add (Endpoint.toSafeId endpoint) errors report
    ) Report.empty krakendEndpoints

let checkKeysExist (keys : List<string>) (krakendEndpoints : array<Krakend.Endpoint>) =
    List.fold (fun report field ->
        (Array.fold (fun report' endpoint ->
            let extendReport err = Report.extend (Endpoint.toSafeId endpoint) [err] report'
            match Map.tryFind field endpoint with
            | Some v ->
                match Util.isString v with
                | true -> report'
                | false -> extendReport $"No string value associated with required key '{field}'"
            | None -> extendReport $"Missing required key '{field}'"
        ) report krakendEndpoints)
    ) Report.empty keys

let makeSortProjection sortKeys =
    fun endpoint -> List.map (fun key -> string (Map.find key endpoint)) sortKeys

let sortEndpoints sortKeys endpoints =
    Seq.sortBy (makeSortProjection sortKeys) endpoints

let makeSortID sortKeys endpoint =
    makeSortProjection sortKeys endpoint |> String.concat ""

let maybeExtendSortReport keys krakend sorted report = 
    match makeSortID keys sorted, makeSortID keys krakend with
    | s, k when s > k ->
        Report.extend
        <| Endpoint.toId krakend
        <| [$"Endpoint should be listed before endpoint '{Endpoint.toId sorted}'"]
        <| report
    | _ -> report

let checkSorting (krakendEndpoints : array<Krakend.Endpoint>) (sortCfg : Kraki.SortConfig) =
    match checkKeysExist sortCfg.by krakendEndpoints with
    | report when not (Report.isEmpty report) -> report
    | report ->
        Seq.fold2 (fun report' krakendEndpoint sortedEndpoint ->
            match krakendEndpoint = sortedEndpoint with
            | true -> report'
            | false -> maybeExtendSortReport sortCfg.by krakendEndpoint sortedEndpoint report'
        ) report krakendEndpoints <| sortEndpoints sortCfg.by krakendEndpoints

let checkEndpoints (krakendEndpoints : array<Krakend.Endpoint>) (endpointCfg : Kraki.EndpointConfig) =
    [ checkFields krakendEndpoints <!> endpointCfg.require;
      checkSorting krakendEndpoints <!> endpointCfg.sort ]
    |> Option.catOptions
    |> Report.merge

let lint (krakiCfg : Kraki.KrakiConfig) (krakendCfg : Krakend.KrakendConfig) =
    let endpoints = krakiCfg.lint >>= fun linterCfg -> linterCfg.endpoints
    match checkEndpoints <!> krakendCfg.endpoints <*> endpoints with
    | Some errorReport when Report.isEmpty errorReport -> Ok ()
    | Some errorReport -> Error errorReport
    | None -> Ok ()
