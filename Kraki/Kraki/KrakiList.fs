module KrakiList

let baseFields = ["endpoint"; "method"]

let private validate validator endpoints =
    List.fold (fun report endpoint ->
        match validator endpoint with
        | [] -> report
        | errors -> Report.extend (Endpoint.toId endpoint) errors report
    ) Report.empty endpoints

let private validateKeysExist (keys: list<string>) (endpoints : list<Endpoint.Endpoint>) =
    validate (Kraki.validateKeysExist keys) endpoints

let listingToReport listing =
    List.fold (fun report (group, endpoints) ->
        let messages = (List.map (Endpoint.toPaddedId >> Message.info) endpoints)
        Report.extend group messages report
    ) Report.empty listing

let listBy field (krakendCfg : Krakend.KrakendConfig) =    
    match krakendCfg.endpoints with
    | None -> Error (Failure "Config file does not contain endpoints")
    | Some endpoints ->
        match validateKeysExist (field :: baseFields) endpoints |> Report.toOption with
        | Some report -> Error (Failure (Report.toErrorMessage report))
        | None ->
            List.groupBy (Map.find field >> string) endpoints
            |> listingToReport
            |> Ok
