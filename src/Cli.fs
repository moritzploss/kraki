module Cli

open CommandLine
open FSharp.Core
open Util.Result

[<Verb("lint", HelpText = "Lint a krakend config file.")>]
type LintArgs =
    { [<Option('c', "config", Default = "kraki.json", HelpText = "Path to a kraki config file.")>]
      configFile: string
      [<Value(0, MetaName = "input file", Required = true, HelpText = "Path to the krakend config file.")>]
      inFile: string }

[<Verb("list", HelpText = "List endpoints in a krakend config file.")>]
type ListArgs =
    { [<Option('g', "groupby", Default = "endpoint", HelpText = "A JSON field by which endpoints are grouped.")>]
      groupBy: string
      [<Value(0, MetaName = "input file", Required = true, HelpText = "Path to the krakend config file.")>]
      inFile: string }

let private runList (listArgs : ListArgs) =
    match Command.list listArgs.groupBy listArgs.inFile with
    | Ok report -> Ok (Report.toStatusMessage report)
    | Error error -> Error error

let private runLint (lintArgs : LintArgs) =
    match Command.lint lintArgs.configFile lintArgs.inFile with
    | Ok (Command.Pass msg) -> Ok msg
    | Ok (Command.Fail report) -> Error (Failure (Report.toErrorMessage report))
    | Error error -> Error error

let private runCommand (parsedArgs: obj) =
    match parsedArgs with
    | :? ListArgs as generateArgs -> runList generateArgs
    | :? LintArgs as lintArgs -> runLint lintArgs

let private parseArgs args =
    match Parser.Default.ParseArguments<ListArgs, LintArgs> args with
    | :? CommandLine.Parsed<obj> as command -> Ok command.Value
    | :? CommandLine.NotParsed<obj> -> Error (Failure "")

let exec args =
    match parseArgs args >>= runCommand with
    | Ok msg ->
        printfn "%s" msg
        0
    | Error exn when exn.Message = "" ->
        1
    | Error exn ->
        printfn "%s" exn.Message
        1
