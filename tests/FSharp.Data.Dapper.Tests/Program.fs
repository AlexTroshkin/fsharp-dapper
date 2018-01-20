module Tests

open Expecto
open FSharp.Data.Dapper

[<EntryPoint>]
let main argv =
    OptionHandler.RegisterTypes()

    InMemoryDatabase.Connection.Initialize()
    let ret = runTestsInAssembly defaultConfig argv
    InMemoryDatabase.Connection.Shutdown()
    
    ret