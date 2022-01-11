module KrakiList

let baseFields = ["endpoint"; "method"]

let listingToReport listing =
    List.fold (fun report (group, endpoints) ->
        Report.extend group (List.map Endpoint.toPaddedId endpoints) report
    ) Report.empty listing

let listBy field (krakendCfg : Krakend.KrakendConfig) =    
    match krakendCfg.endpoints with
    | None -> Error (Failure "Config file does not contain endpoints")
    | Some endpoints ->
        match KrakiLint.checkKeysExist (field :: baseFields) endpoints with
        | report when not (Report.isEmpty report) ->
            Error (Failure (Report.toString Report.Error report))
        | _ ->
            Array.toList endpoints
            |> List.groupBy (Map.find field >> string)
            |> listingToReport
            |> Ok
