module Message

type Message =
    MissingKeyError of string
    | SchemaMismatchError of string
    | WrongOrderError of string
    | Info of string

let toString message =
    match message with
    | MissingKeyError e -> e
    | SchemaMismatchError e -> e
    | WrongOrderError e -> e
    | Info e -> e

let missingKey field =
    MissingKeyError $"Missing required key '{field}'"

let schemaMismatch schema errors =
    let description =
        match Map.tryFind "title" schema with
        | Some title -> $"schema '{title}'"
        | None -> "untitled schema"
    SchemaMismatchError $"Definition does not match {description}: {errors}"

let wrongSortOrder sortKeys expected =
    let sortOrder = String.concat " -> " sortKeys
    WrongOrderError $"Should be defined after '{expected}'. Expected sort order: {sortOrder}"

let info message =
    Info message
