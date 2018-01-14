module Fixtures

open Expecto

open FSharp.Data.Dapper
open FSharp.Data.Dapper.Query.Parameters

open InMemoryDatabase.Types
open InMemoryDatabase.Scripts
open InMemoryDatabase.Connection

open System.Data

let ``with connection`` testFunc () =
    use connection = CreateDedicatedConnection None
    
    testFunc connection

let ``connection with empty person table`` testFunc () =
    let withScripts = Some [ ``create person table`` ]
    use connection = CreateDedicatedConnection withScripts

    testFunc connection
        
let ``connection with filled person table`` testFunc () =
    let withScripts = Some [``create person table``; ``insert persons`` ]
    use connection = CreateDedicatedConnection withScripts

    testFunc connection            
