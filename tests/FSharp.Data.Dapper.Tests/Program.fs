module Tests

open Expecto

[<EntryPoint>]
let main argv =
    InMemoryDatabase.Initialize()
    let ret = runTestsInAssembly defaultConfig argv
    InMemoryDatabase.Shutdown()
    
    ret