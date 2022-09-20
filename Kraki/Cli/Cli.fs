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
      [<Option('f', "format", Default = "padded", HelpText = "The output format of the endpoint list ('padded' or 'json').")>]
      format: string
      [<Value(0, MetaName = "input file", Required = true, HelpText = "Path to the krakend config file.")>]
      inFile: string }

let private runCommand (parsedArgs: obj) =
    match parsedArgs with
    | :? ListArgs as args -> Command.list args.groupBy args.format args.inFile
    | :? LintArgs as args -> Command.lint args.configFile args.inFile

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
