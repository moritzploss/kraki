module Kraki

open NJsonSchema
open FSharp.Json
open Util

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

let validate schema (str : string) =
    let validator = getValidator schema
    match Result.ofFailable <| lazy validator.Validate str with
    | Ok seq ->
        seq
        |> Seq.toList
        |> List.map (fun e -> e.ToString().Replace(": #/", ": "))
        |> List.map (Message.schemaMismatch schema)
    | Error exn ->
        match exn.Message with
        | msg when msg.StartsWith "Invalid pattern" -> [Message.regexError schema msg]
        | msg -> [Message.info msg]

let validateSchema schema value =
    Json.serialize(value) |> validate schema

let parseConfig filePath =
    Parser.parseLax<KrakiConfig> filePath
