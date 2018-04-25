module QueryWithTemporaryTableTests

open Expecto


open FSharp.Data.Dapper
open FSharp.Data.Dapper.TempTable
open FSharp.Data.Dapper.Query.Parameters

// open Fixtures
// open Enums

// [<Tests>]
// let queryWithTemporaryTableTests =
//     testList "Query with temp table tests" [
        
//         yield! testFixture ``connection with filled person table`` [
//             "Must find person by inner join with temp table",
//             fun connection ->
//                 let tempTable = ``Temp table with one column``("PersonIdentificators", "Id", [1])
//                 let personIdentificators = TempTable.Create tempTable DatabaseType.Sqlite
//                 let script = "select * from Person as p inner join PersonIdentificators as pi on p.Id = pi.Id"

//                 let persons = 
//                     Query (script, temporaryTables = [personIdentificators])
//                     |> QueryAsync<Person> <| connection
//                     |> Async.RunSynchronously
//                     |> List.ofSeq

//                 Expect.isNonEmpty persons "Not found person by inner join with temp table"
//         ]

//         yield! testFixture ``with connection`` [
//             "Must return all persons from temp table",
//             fun connection ->
//                 let persons = [{ Id = 1L; Name = "Ivan"; Patronymic = Some "Ivanovich"; Surname = "Ivanov" }]
//                 let script = "select * from TPerson"
//                 let tempTable = ``Temp table``("TPerson", persons)
//                 let personsTempTable = TempTable.Create tempTable DatabaseType.Sqlite

//                 let persons = 
//                     Query (script, temporaryTables = [personsTempTable])
//                     |> QueryAsync<Person> <| connection
//                     |> Async.RunSynchronously
//                     |> List.ofSeq

//                 Expect.isNonEmpty persons "Not found person by inner join with temp table"
//         ]
//     ]