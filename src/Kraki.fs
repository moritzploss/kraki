module Kraki

open NJsonSchema
open FSharp.Json

type Schema = Map<string, obj>

type SortConfig = {
    by: List<string>
    order: option<string>
}

type EndpointConfig = {
    schema: option<Schema>
    sort: option<SortConfig>
}

type LinterConfig = {
    endpoints: option<EndpointConfig>
}

type KrakiConfig = {
    lint: option<LinterConfig>
}

let private getValidator schema =
    JsonSchema.FromJsonAsync(Json.serialize(schema))
        |> Async.AwaitTask
        |> Async.RunSynchronously

let addError errorFun ctx endpoint report =
    Report.extend (Endpoint.toSafeId endpoint) [errorFun "Endpoint" ctx] report

let checkKeysExist (keys : List<string>) (endpoints : list<Endpoint.Endpoint>) (report : Report.Report) =
    List.fold (fun report' key ->
        List.fold (fun report'' endpoint ->
            match Map.containsKey key endpoint with
            | true -> report''
            | false -> addError KrakiError.missingRequiredField key endpoint report''
        ) report' endpoints
    ) report keys

let checkSchema schema endpoints report =
    let validator = getValidator schema
    List.fold (fun report' endpoint ->
        match Json.serialize(endpoint) |> validator.Validate with
        | seq when Seq.isEmpty seq -> report'
        | seq -> addError (KrakiError.schemaError (Seq.toList seq)) schema endpoint report'
    ) report endpoints

let parseConfig filePath =
    Config.parseLax<KrakiConfig> filePath
