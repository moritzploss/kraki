module Util

let isString obj =
    match box obj with
    | :? string -> true
    | _ -> false

module Option =

    let ofFailable (f : Lazy<'T>) =
        try
            Some f.Value
        with
        | _ -> None

    let apply (fOpt : option<'a -> 'b>) (xOpt : option<'a>)=
        match fOpt, xOpt with
        | Some f, Some x -> Some (f x)
        | _ -> None

    let (<*>) = apply

    let (<!>) = Option.map

    let (>>=) a b = Option.bind b a

    let catOptions l = List.filter (Option.isSome) l |> List.map Option.get

module Result =

    let ofFailable (f : Lazy<'T>) =
        try
            Ok(f.Value)
        with
        | ex -> Error(ex)

    let apply (fRes : Result<'a -> 'b,'c>) (xRes : Result<'a,'c>) =
        match fRes, xRes with
        | Ok f, Ok x -> Ok (f x)
        | Error e, _ -> Error e
        | _, Error e -> Error e

    let (<*>) = apply

    let (<!>) = Result.map

    let (>>=) a b = Result.bind b a

    let isError result =
        match result with
        | Ok _ -> false
        | Error _ -> true

    let isOk result = isError >> not <| result

    let get result =
        match result with
        | Ok a -> a
        | Error _ -> failwith "excepted type constructor 'Ok', but got 'Error'"

    let error result =
        match result with
        | Error a -> a
        | Ok _ -> failwith "excepted type constructor 'Error', but got 'Ok'"
