module QuerySignleTests

open Expecto

open FSharp.Data.Dapper.Query
open FSharp.Data.Dapper.Query.Parameters

open InMemoryDatabase.Types
open InMemoryDatabase.Scripts
open InMemoryDatabase.Connection

open System.Data

[<Tests>]
let querySingleTests = 
    testList "query single tests" [

        let ``with empty person table`` testFunc () =
            let withScripts = Some [ ``create person table`` ]
            use connection = CreateDedicatedConnection withScripts

            testFunc connection
        
        let ``with filled person table`` testFunc () =
            let withScripts = Some [``create person table``; ``insert persons`` ]
            use connection = CreateDedicatedConnection withScripts

            testFunc connection            

        yield! testFixture ``with empty person table`` [

            "Must return Some when query count", 
            fun connection -> 

                let script = "select count(1) from Person"

                let countOfPersons = 
                    { DefaultQueryDefinition with Script = script }
                    |> QuerySingleAsync <| connection
                    |> Async.RunSynchronously

                Expect.isSome countOfPersons "count of persons not Some(0)"
                Expect.equal countOfPersons.Value 0 "count of persons not equal 0"

            "Must return None when person not found",
            fun connection ->

                let parameters = Parameters.Create [ "Id" <=> 1 ]
                let script = "select * from Person where Id = @Id"

                let person = 
                    { DefaultQueryDefinition with Script = script ; Parameters = parameters }
                    |> QuerySingleAsync <| connection
                    |> Async.RunSynchronously

                Expect.isNone person "got Some instead None from empty table"            
        ]

        yield! testFixture ``with filled person table`` [
            
            "Must return Some when person found",
            fun connection -> 

                let parameters = Parameters.Create [ "Id" <=> 1 ]
                let script = "select * from Person where Id = @Id"
                
                let person = 
                    { DefaultQueryDefinition with Script = script ; Parameters = parameters }
                    |> QuerySingleAsync <| connection
                    |> Async.RunSynchronously
                
                Expect.isSome person "got Some instead None from empty table"
        ]
    ]