module QuerySignleOptionAsyncTests

open Expecto
open SqliteDatabase.Types
open SqliteDatabase.Queries

[<Tests>]
let querySignleOptionAsyncTests = 
    testList "QuerySignleOptionAsyncTests" [
        testAsync "Must return Some when Person is found" {
            let! personOpt = querySingleOptionAsync<Person> {
                script "select * from Person where Name = @Name limit 5"
                parameters (dict ["Name", box "Иван"])
            }

            Expect.equal personOpt.IsSome true "personOpt = None"
        }

        testAsync "Must return None when Person is not found" {
            let! personOpt = querySingleOptionAsync<Person> {
                script "select * from Person where Name = @Name limit 5"
                parameters (dict ["Name", box "NONEXISTENT_NAME"])
            }

            Expect.equal personOpt.IsNone true "personOpt = Some"
        }
    ]