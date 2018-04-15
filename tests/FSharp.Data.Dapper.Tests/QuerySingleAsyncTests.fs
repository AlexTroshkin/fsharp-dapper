module QuerySingleAsyncTests

open Expecto
open SqliteDatabase.Types
open SqliteDatabase.Queries

[<Tests>]
let querySingleAsyncTests = 
    testList "QuerySingleAsyncTests" [
        testAsync "Must return Person if found" {
            let! person = querySingleAsync<Person> {
                script "select * from Person where Name = @Name limit 5"
                parameters (dict ["Name", box "Иван"])
            }

            Expect.isNotNull (box person) "person is null"
        }

        testAsync "Must throw exception when Person is not found" {
            Expect.throws (fun _ -> 
                querySingleAsync<Person> {
                    script "select * from Person where Name = @Name limit 5"
                    parameters (dict ["Name", box "NONEXISTENT_NAME"]) } 
                |> Async.RunSynchronously 
                |> ignore
            ) "There was no exception when Person was not found"
        }
    ]