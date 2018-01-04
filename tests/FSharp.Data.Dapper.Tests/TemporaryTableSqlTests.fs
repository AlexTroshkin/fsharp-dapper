module TemporaryTableSqlTests

open Expecto
open InMemoryDatabase.Types

open FSharp.Data.Dapper
open FSharp.Data.Dapper.TemporaryTable

[<Tests>]
let tests =
    testList "Temporary table sql tests" [
        let ``with metadata of person table`` testFunc () =
            let rows = [{ Id = 1L; Name = ""; Patronymic = None; Surname = "" }]
            let table = { Name = "Persons"; Rows = rows }
            let metadata = Metadata.Create table

            testFunc metadata

        yield! testFixture ``with metadata of person table`` [
            "Verify create table script",
            fun metadata -> 
                let expectedSql = "create table #Persons (Id bigint not null,Name nvarchar(max) not null,Patronymic nvarchar(max) null,Surname nvarchar(max) not null)"
                
                let actualSql = Sql.CreateScript metadata

                Expect.equal actualSql expectedSql "Create scripts are not equals"

            "Verify drop table script",
            fun metadata ->
                let expectedSql = "drop table #Persons"
                
                let actualSql = Sql.DropScript metadata

                Expect.equal actualSql expectedSql "Drop scripts are not equals"
        ]
    ]