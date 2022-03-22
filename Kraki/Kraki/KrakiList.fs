module KrakiList

let baseFields = ["endpoint"]

let private validateKeysExist (keys: list<string>) (endpoints : list<Endpoint.Endpoint>) =
    Endpoint.validate (Kraki.validateKeysExist keys) endpoints

let listingToReport listing =
    List.fold (fun report (group, endpoints) ->
        let messages = (List.map (Endpoint.toPaddedId >> Message.info) endpoints)
        Report.extend group messages report
    ) Report.empty listing

let listBy field (krakendCfg : Krakend.KrakendConfig) =    
    match krakendCfg.endpoints with
    | None -> Error (Report.extend "Config file does not contain endpoints" [] Report.empty)
    | Some endpoints ->
        match validateKeysExist (field :: baseFields) endpoints |> Report.toOption with
        | Some report -> Error report
        | None ->
            List.groupBy (Map.find field >> string) endpoints
            |> listingToReport
            |> Ok
