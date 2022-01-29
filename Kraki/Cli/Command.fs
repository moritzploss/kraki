module Command

open Util.Result

type LintResult = Pass of string | Fail of Report.Report

let private loadConfig configParser path =
    File.read path >>= configParser

let formatResult result =
    match result with
    | Ok (Ok report) -> Ok (Report.toStatusMessage report)
    | Ok (Error report) -> Error (Failure (Report.toErrorMessage report))
    | Error reason -> Error reason

let list (groupBy: string) (krakendFile : string) =
    let krakendCfg = loadConfig Krakend.parseConfig krakendFile
    KrakiList.listBy groupBy <!> krakendCfg |> formatResult

let lint (krakiFile : string) (krakendFile : string) =
    let krakendConfig = loadConfig Krakend.parseConfig krakendFile
    let krakiConfig = loadConfig Kraki.parseConfig krakiFile
    KrakiLint.lint <!> krakiConfig <*> krakendConfig |> formatResult
