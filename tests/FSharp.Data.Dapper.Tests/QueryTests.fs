module QueryTests

open Expecto
open FSharp.Data.Dapper.Query
open InMemoryDatabase

[<Tests>]
let queryTests = 
    testList "query tests" []