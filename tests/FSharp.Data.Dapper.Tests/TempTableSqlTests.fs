module TempTableSqlTests

open Expecto
open SqliteDatabase.Types

open FSharp.Data.Dapper.Enums
open FSharp.Data.Dapper.TempTable

[<Tests>]
let tests =
    testList "Temporary table sql tests" [
        let ``with schema of person table`` testFunc () =
            let rows = [{ Id = 1L; Name = ""; Patronymic = None; Surname = "" }]
            let schemaOfTable = Schema.Create Sqlite "TPersons" rows

            testFunc schemaOfTable

        yield! testFixture ``with schema of person table`` [
            "Verify create table script",
            fun schemaOfTable -> 
                let expectedSql = "create temp table TPersons (Id BigInt not null,Name NText not null,Patronymic NText null,Surname NText not null)"
                let actualSql = Sql.CreateScript Sqlite schemaOfTable

                Expect.equal actualSql expectedSql "Create scripts are not equals"

            "Verify drop table script",
            fun schemaOfTable ->
                let expectedSql = "drop table TPersons"                
                let actualSql = Sql.DropScript schemaOfTable

                Expect.equal actualSql expectedSql "Drop scripts are not equals"
        ]
    ]