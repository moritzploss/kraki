module Report

type private ReportMessages = List<Message.Message>

type private ReportType = Status | Error

type Report = Map<string,ReportMessages>

let private countMessages report =
    Map.fold (fun c _ messages -> c + List.length messages) 0 report

let private toString (reportType : ReportType) (report : Report) : string =
    let msg =
        Map.fold (fun acc key messages ->
            match List.map Message.toString messages with
            | [] -> key :: acc
            | strMessages -> (key + "\n  " + (String.concat "\n  " (List.sort strMessages))) :: acc
        ) [] report
        |> List.sort
        |> String.concat "\n\n"
    match reportType with
    | Error ->
        let count = countMessages report
        let firstLine = $"\nFound {count} error" + (if count = 1 then "" else "s")
        firstLine + "\n\n" + msg
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

let toErrorMessage (report: Report) : string =
    toString Error report

let toStatusMessage (report: Report) : string =
    toString Status report
