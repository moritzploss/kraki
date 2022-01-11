module Command

open Util.Result

type OutType = OpenApi

type LintResult = Pass of string | Fail of Report.Report

let loadConfig configParser path =
    File.read path >>= configParser

let list (groupBy: string) (krakendFile : string)  =
    loadConfig Krakend.parseConfig krakendFile >>= KrakiList.listBy groupBy

let lint (krakiFile : string) (krakendFile : string) =
    let krakendConfig = loadConfig Krakend.parseConfig krakendFile
    let krakiConfig = loadConfig Kraki.parseConfig krakiFile
    match KrakiLint.lint <!> krakiConfig <*> krakendConfig with
    | Ok (Ok ()) -> Ok (Pass "Linter OK!")
    | Ok (Error report) -> Ok (Fail report)
    | Error reason -> Error(reason)
