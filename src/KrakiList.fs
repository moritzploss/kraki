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
        match Kraki.checkKeysHaveStringValue (field :: baseFields) endpoints Report.empty |> Report.toOption with
        | Some report -> Error (Failure (Report.toErrorMessage report))
        | None ->
            List.groupBy (Map.find field >> string) endpoints
            |> listingToReport
            |> Ok
