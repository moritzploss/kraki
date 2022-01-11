module Krakend

type Endpoint = Map<string, obj>

type KrakendConfig = {
    version: int
    name: string
    endpoints: option<array<Endpoint>>
}

let parseConfig filePath =
    Config.parseLax<KrakendConfig> filePath
