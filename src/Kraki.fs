module Kraki

open NJsonSchema
open FSharp.Json

type Validator = obj -> Result<unit, obj -> string>

type SortConfig = {
    by: List<string>
    order: option<string>
}

type EndpointConfig = {
    require: option<Map<string, obj>>
    sort: option<SortConfig>
}

type LinterConfig = {
    endpoints: option<EndpointConfig>
}

type KrakiConfig = {
    lint: option<LinterConfig>
}

let private valueIsString value =
    match Util.isString value with
    | true -> Ok ()
    | false -> Error KrakiError.noStringValueAssociated

let private valueHasSchema schemaTemplate value =
    match Util.isString schemaTemplate, Util.isString value with
    | true, true -> Ok ()
    | true, _ -> Error (KrakiError.schemaError schemaTemplate)
    | _, true -> Error (KrakiError.schemaError schemaTemplate)
    | _, _ ->
        let schema = JsonSchema.FromSampleJson(Json.serialize(schemaTemplate))
        match Json.serialize(value) |> schema.Validate |> Seq.length with
        | 0 -> Ok ()
        | _ -> Error (KrakiError.schemaError schemaTemplate)

let private validateOne (validator: Validator) (key : string) (endpoint: Endpoint.Endpoint) (report : Report.Report) =
    let extendReport error =
        let id = Endpoint.toSafeId endpoint
        Report.extend id [error] report
    match Map.tryFind key endpoint with
    | Some v -> 
        match validator v with
        | Ok _ -> report
        | Error errorFun -> extendReport (errorFun v)
    | None -> extendReport (KrakiError.missingRequiredField key)

let private validate (validator: Validator) (keys : List<string>) (endpoints : list<Endpoint.Endpoint>) (report : Report.Report) =
    List.fold (fun report' key ->
        List.fold (fun report'' endpoint ->
            validateOne validator key endpoint report''
        ) report' endpoints
    ) report keys

let checkKeysHaveStringValue (keys : List<string>) (endpoints : list<Endpoint.Endpoint>) report =
    validate valueIsString keys endpoints report

let checkValuesHaveSchema schemaTemplate (keys : List<string>) (endpoints : list<Endpoint.Endpoint>) report =
    validate (valueHasSchema schemaTemplate) keys endpoints report

let parseConfig filePath =
    Config.parseLax<KrakiConfig> filePath
