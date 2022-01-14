module Krakend

type KrakendConfig = {
    version: int
    name: string
    endpoints: option<list<Endpoint.Endpoint>>
}

let parseConfig filePath =
    Config.parseLax<KrakendConfig> filePath
