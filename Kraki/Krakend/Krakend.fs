module Krakend

type KrakendConfig = {
    endpoints: option<list<Endpoint.Endpoint>>
}

let parseConfig filePath =
    Parser.parseLax<KrakendConfig> filePath
