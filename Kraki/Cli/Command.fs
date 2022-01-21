module Command

open Util.Result

type LintResult = Pass of string | Fail of Report.Report

let private loadConfig configParser path =
    File.read path >>= configParser

let list (groupBy: string) (krakendFile : string)  =
    loadConfig Krakend.parseConfig krakendFile >>= KrakiList.listBy groupBy
    |> Result.map Report.toStatusMessage

let lint (krakiFile : string) (krakendFile : string) =
    let krakendConfig = loadConfig Krakend.parseConfig krakendFile
    let krakiConfig = loadConfig Kraki.parseConfig krakiFile
    match KrakiLint.lint <!> krakiConfig <*> krakendConfig with
    | Ok (Ok ()) -> Ok "Linter OK!"
    | Ok (Error report) -> Error (Failure (Report.toErrorMessage report))
    | Error reason -> Error reason
