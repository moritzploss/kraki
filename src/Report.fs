module Report

type private ReportMessages = List<string>

type private ReportType = Status | Error

type Report = Map<string,ReportMessages>

let private toString (reportType : ReportType) (report : Report) : string =
    let msg =
        Map.fold (fun acc key messages ->
            (key + "\n  " + (String.concat "\n  " messages)) :: acc
        ) [] report
        |> String.concat "\n\n"
    match reportType with
    | Error ->
        let count = Map.fold (fun c _ messages -> c + List.length messages) 0 report
        $"\nFound {count} error" + (if count = 1 then "" else "s") + "\n\n" + msg
    | Status -> msg

let empty : Report = Map.empty

let isEmpty (report : Report) =
    Map.isEmpty report

let toOption (report: Report) =
    match isEmpty report with
    | true -> None
    | false -> Some report

let extend (key : string) (moreMessages : ReportMessages) (report : Report) : Report =
    Map.change key (fun existingMessages ->
        match existingMessages with
        | Some messages -> Some (List.concat (messages :: [moreMessages]))
        | None -> Some moreMessages
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

let toErrorMessage (report: Report) : string =
    toString Error report

let toStatusMessage (report: Report) : string =
    toString Status report
