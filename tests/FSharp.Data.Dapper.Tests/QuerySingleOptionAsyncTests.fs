module QuerySignleOptionAsyncTests

open Expecto
open SqliteDatabase.Queries

[<Tests>]
let querySignleOptionAsyncTests = 
    testList "QuerySignleOptionAsyncTests" [
        testAsync "Must return Some when Person is found" {
            let! personOpt = Person.TryFindByName "Иван"
            Expect.equal personOpt.IsSome true "personOpt = None"
        }

        testAsync "Must return None when Person is not found" {
            let! personOpt = Person.TryFindByName "NONEXISTENT_NAME"
            Expect.equal personOpt.IsNone true "personOpt = Some"
        }
    ]