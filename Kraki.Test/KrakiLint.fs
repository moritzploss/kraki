namespace Kraki.Test

open Microsoft.VisualStudio.TestTools.UnitTesting

open Util

[<TestClass>]
type TestClass () =

    let krakiCfg sortBy =
        let sortCfg = { Kraki.by = sortBy; Kraki.order = None }
        let endpointCfg = { Kraki.schema = None; Kraki.sort = Some sortCfg }
        let linterCfg = { Kraki.endpoints = Some endpointCfg }
        { Kraki.lint = Some linterCfg }

    let endpointCfg schema =
        { Kraki.schema = Some schema; Kraki.sort = None }

    let krakendCfg endpoints =
        let endpoints' : List<Map<string,obj>> =
            List.map (fun vals ->
                List.fold (fun acc (k, v) -> Map.add k v acc) Map.empty vals
            ) endpoints
        { Krakend.endpoints = Some endpoints' }

    let getErrors key report =
        match Map.tryFind key report with
        | Some errors -> errors
        | None -> []

    let isWrongOrderError message =
        match message with
        | Message.WrongOrderError _ -> true
        | _ -> false

    let isMissingKeyError message =
        match message with
        | Message.MissingKeyError _ -> true
        | _ -> false

    let isSchemaMismatchError message =
        match message with
        | Message.SchemaMismatchError _ -> true
        | _ -> false

    let sampleSchema =
        Map.empty
            .Add("$schema", box "https://json-schema.org/draft/2020-12/schema")
            .Add("title", box "Endpoint")
            .Add("type", box "object")
            .Add("properties",
                Map.empty
                    .Add("@owner", Map.empty.Add("type", "string"))
                    .Add("@group", Map.empty.Add("type", "string" ))
            )
            .Add("required", ["@owner"; "@group"])

    [<TestMethod>]
    member this.TestCorrectlySortedEndpoints () =
        let krakend = krakendCfg [
            [("method", "GET"); ("endpoint", "a")];
            [("method", "GET"); ("endpoint", "b")];
            [("method", "GET"); ("endpoint", "c")]
        ]
        let kraki = krakiCfg ["endpoint"]

        let actual = KrakiLint.lint kraki krakend

        let expected : Result<unit, Report.Report> = Ok ()
        Assert.AreEqual(expected, actual)

    [<TestMethod>]
    member this.TestWronglySortedByEndpoint () =
        let krakend = krakendCfg [
            [("method", "GET"); ("endpoint", "b")];
            [("method", "GET"); ("endpoint", "a")];
            [("method", "GET"); ("endpoint", "c")]
        ]
        let kraki = krakiCfg ["endpoint"]      
  
        let report = KrakiLint.lint kraki krakend

        Assert.IsTrue(Result.isError report)
        let bID = Option.some krakend.endpoints |> List.head |> Endpoint.toId
        let bErrors = getErrors bID (report |> Result.error)
        Assert.AreEqual(1, List.length bErrors)
        Assert.IsTrue(isWrongOrderError <| List.head bErrors)

    [<TestMethod>]
    member this.TestWronglySortedByEndpointAndMethod () =
        let krakend = krakendCfg [
            [("method", "GET"); ("endpoint", "a")];
            [("method", "PUT"); ("endpoint", "a")];
            [("method", "POST"); ("endpoint", "a")]
        ]
        let kraki = krakiCfg ["endpoint"; "method"]      
  
        let report = KrakiLint.lint kraki krakend

        Assert.IsTrue(Result.isError report)
        let putID = Option.some krakend.endpoints |> List.item 1 |> Endpoint.toId
        let putErrors = getErrors putID (report |> Result.error)
        Assert.AreEqual(1, List.length putErrors)
        Assert.IsTrue(isWrongOrderError <| List.head putErrors)

    [<TestMethod>]
    member this.TestMissingSortKey () =
        let krakend = krakendCfg [
            [("method", "GET"); ("endpoint", "b")];
            [("method", "GET"); ("endpoint", "a")];
            [("method", "GET"); ("endpoint", "c")]
        ]
        let kraki = krakiCfg ["missing"]      
  
        let report = KrakiLint.lint kraki krakend

        Assert.IsTrue(Result.isError report)
        let errors =
            Option.some krakend.endpoints
            |> List.map Endpoint.toId
            |> List.map (fun ID -> getErrors ID <| Result.error report)
            |> List.concat
        Assert.AreEqual(3, List.length errors)
        Assert.IsTrue(List.forall isMissingKeyError errors)

    [<TestMethod>]
    member this.TestValidEndpointSchema () =
        let krakend = krakendCfg [
            [("method", "GET"); ("endpoint", "a"); ("@owner", "o"); ("@group", "g")];
            [("method", "GET"); ("endpoint", "b"); ("@owner", "o"); ("@group", "g")];
            [("method", "GET"); ("endpoint", "c"); ("@owner", "o"); ("@group", "g")]
        ]
        let kraki = { Kraki.lint = Some { Kraki.endpoints = Some (endpointCfg sampleSchema) } }
  
        let actual = KrakiLint.lint kraki krakend

        let expected : Result<unit, Report.Report> = Ok ()
        Assert.AreEqual(expected, actual)

    [<TestMethod>]
    member this.TestInvalidEndpointSchema () =
        let krakend = krakendCfg [
            [("method", "GET"); ("endpoint", "a"); ("@owner", "o"); ("@group", "g")];
            [("method", "GET"); ("endpoint", "b"); ("@owner", ["o"]); ("@group", "g")];
            [("method", "GET"); ("endpoint", "c"); ("@owner", "o")]
        ]
        let kraki = { Kraki.lint = Some { Kraki.endpoints = Some (endpointCfg sampleSchema) } }
  
        let report = KrakiLint.lint kraki krakend

        Assert.IsTrue(Result.isError report)
        let errors =
            Option.some krakend.endpoints
            |> List.map Endpoint.toId
            |> List.map (fun ID -> getErrors ID <| Result.error report)
            |> List.concat
        Assert.AreEqual(2, List.length errors)
        Assert.IsTrue(List.forall isSchemaMismatchError errors)
