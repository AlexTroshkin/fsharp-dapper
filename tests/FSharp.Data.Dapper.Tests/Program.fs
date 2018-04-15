module Tests

open Expecto
open FSharp.Data.Dapper

[<EntryPoint>]
let main argv =
    OptionHandler.RegisterTypes()

    SqliteDatabase.Run (fun _ ->
        runTestsInAssembly defaultConfig argv
    )