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

let validateKeysExist keys value  =
    List.fold (fun errors key ->
        match Map.containsKey key value with
        | true -> errors
        | false -> Message.missingKey key :: errors
    ) [] keys

let validateSchema schema value =
    let validator = getValidator schema
    Json.serialize(value)
    |> validator.Validate
    |> Seq.toList
    |> List.map (fun e -> e.ToString().Replace(": #/", ": "))
    |> List.map (Message.schemaMismatch schema)

let parseConfig filePath =
    Parser.parseLax<KrakiConfig> filePath
