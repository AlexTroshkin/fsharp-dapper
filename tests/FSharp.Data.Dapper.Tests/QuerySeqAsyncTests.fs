module QuerySeqAsyncTests

open Expecto
open SqliteDatabase.Queries

[<Tests>]
let querSeqAsyncTests = 
    testList "querySeqAsync tests" [
        testAsync "Must return Person if found" {
            let! person = Person.FindByName "Иван"
            Expect.isNotNull (box person) "person is null"
        }

        testAsync "Must throw exception when Person is not found" {
            Expect.throws (fun _ -> 
                Person.FindByName "NONEXISTENT_NAME"
                |> Async.RunSynchronously 
                |> ignore
            ) "There was no exception when Person was not found"
        }
    ]