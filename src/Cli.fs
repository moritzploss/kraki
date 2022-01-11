module Cli

open CommandLine
open Util.Result

exception ParseError of string

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

let runList (listArgs : ListArgs) =
    match Command.list listArgs.groupBy listArgs.inFile with
    | Ok report -> Ok <| Report.toString Report.Success report
    | Error error -> Result.Error error

let runLint (lintArgs : LintArgs) =
    match Command.lint lintArgs.configFile lintArgs.inFile with
    | Ok (Command.Pass msg) -> Ok msg
    | Ok (Command.Fail report) -> Result.Error (Failure (Report.toString Report.Error report))
    | Error error -> Result.Error error

let runCommand (command: CommandLine.Parsed<obj>) =
    match command.Value with
    | :? ListArgs as generateArgs -> runList generateArgs
    | :? LintArgs as lintArgs -> runLint lintArgs

let parseArgs args =
    match Parser.Default.ParseArguments<ListArgs, LintArgs> args with
    | :? CommandLine.Parsed<obj> as command -> Ok command
    | :? CommandLine.NotParsed<obj> as command -> Core.Error(ParseError <| (Seq.head command.Errors).ToString())

let exec args =
    match parseArgs args >>= runCommand with
    | Ok msg ->
        printfn "%s" msg
        0
    | Error exn ->
        printfn "%s" exn.Message
        1
