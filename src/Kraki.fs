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
