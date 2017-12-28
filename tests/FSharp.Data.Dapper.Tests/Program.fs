module Tests

open Expecto

[<EntryPoint>]
let main argv =
    InMemoryDatabase.Connection.Initialize()
    let ret = runTestsInAssembly defaultConfig argv
    InMemoryDatabase.Connection.Shutdown()
    
    ret