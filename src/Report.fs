module Report

type ReportMessages = List<string>

type Report = Map<string,ReportMessages>

type ReportType = Error | Success

let empty : Report = Map.empty

let isEmpty (report : Report) = Map.isEmpty report

let extend (key : string) (moreErrors : ReportMessages) (report : Report) : Report =
    Map.change key (fun existingErrors ->
        match existingErrors with
        | Some errors -> Some (List.concat (errors :: [moreErrors]))
        | None -> Some moreErrors
    ) report

let merge (reports : List<Report>) : Report =
    List.map Map.keys reports
    |> List.map Seq.toList
    |> List.concat
    |> List.distinct
    |> List.fold (fun combinedReport key ->
        List.fold (fun combinedReport' report ->
            match Map.tryFind key report with
            | Some moreErrors -> extend key moreErrors combinedReport'
            | None -> combinedReport'
        ) combinedReport reports
    ) Map.empty

let toString (reportType : ReportType) (report : Report) : string =
    let count, msg =
        Map.fold (fun (c, acc) k errs ->
            let errorList = String.concat "\n  " errs
            (c + List.length errs, acc + $"\n\n{k}:\n  {errorList}")
        ) (0, "") report
    match reportType with
    | Error -> $"\nFound {count} error" + (if count = 1 then "" else "s") + msg
    | Success -> msg
