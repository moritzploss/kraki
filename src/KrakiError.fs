module KrakiError

let missingRequiredField valueType field =
    $"{valueType} is missing required key '{field}'"

let schemaError errors valueType schema  =
    let description =
        match Map.tryFind "title" schema with
        | Some title -> $"schema '{title}'"
        | None -> "untitled schema"
    $"{valueType} definition does not match {description}: {errors}"

let incorrectSortOrder valueType expected =
    $"{valueType} should be listed before endpoint '{expected}'"
