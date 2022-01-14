module Kraki

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

let parseConfig filePath =
    Config.parseLax<KrakiConfig> filePath

let checkKeysHaveStringValue (keys : List<string>) (krakendEndpoints : array<Endpoint.Endpoint>) =
    List.fold (fun report field ->
        (Array.fold (fun report' endpoint ->
            let extendReport err = Report.extend (Endpoint.toSafeId endpoint) [err] report'
            match Map.tryFind field endpoint with
            | Some v ->
                match Util.isString v with
                | true -> report'
                | false -> extendReport <| KrakiError.noStringValueAssociated field
            | None -> extendReport <| KrakiError.missingRequiredField field
        ) report krakendEndpoints)
    ) Report.empty keys
