module Message

type Message =
    MissingKeyError of string
    | SchemaMismatchError of string
    | RegexError of string
    | WrongOrderError of string
    | Info of string

let private getTitle schema =
    match Map.tryFind "title" schema with
    | Some title -> $"schema '{title}'"
    | None -> "untitled schema"

let toString message =
    match message with
    | MissingKeyError e -> e
    | SchemaMismatchError e -> e
    | RegexError e -> e
    | WrongOrderError e -> e
    | Info e -> e

let missingKey field =
    MissingKeyError $"Missing required key '{field}'"

let schemaMismatch schema errors =
    SchemaMismatchError $"Definition does not match {getTitle schema}: {errors}"

let regexError schema error =
    RegexError $"Regex error in {getTitle schema}: {error}"

let wrongSortOrder sortKeys expected =
    let sortOrder = String.concat " -> " sortKeys
    WrongOrderError $"Should be defined after '{expected}'. Expected sort order: {sortOrder}"

let info message =
    Info message
