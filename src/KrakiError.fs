module KrakiError

let noStringValueAssociated field =
    $"No string value associated with required key '{field}'"

let missingRequiredField field =
    $"Missing required key '{field}'"

let schemaError schemaTemplate value =
    $"Value '{value}' does not match schema template '{schemaTemplate}'"

let incorrectSortOrder expected =
    $"Endpoint should be listed before endpoint '{expected}'"
