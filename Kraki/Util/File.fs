module File

open System.IO
open Util

let read path =
    Result.ofFailable <| lazy File.ReadAllText(path)

let write path contents =
    Result.ofFailable <| lazy File.WriteAllText(path, contents)
