module TemporaryTableMetadataTests

open Expecto

open InMemoryDatabase.Types

open System.Data

open FSharp.Data.Dapper
open FSharp.Data.Dapper.TemporaryTable

[<Tests>]
let tests = 
    testList "Temporary table metadata tests" [
        test "Verify person temporary table metadata" {
            let rows = [{ Id = 1L; Name = "" ; Patronymic = None; Surname = "" }]

            let metadata = Metadata.Create "Persons" rows
            let expectedColumns = [
                { Name = "Id"        ; SqlTypeAsString = "bigint"       ; AllowNull = false; ClrType = typeof<int64>  ; SqlType = DbType.Int64 }
                { Name = "Name"      ; SqlTypeAsString = "nvarchar(max)"; AllowNull = false; ClrType = typeof<string> ; SqlType = DbType.String }
                { Name = "Patronymic"; SqlTypeAsString = "nvarchar(max)"; AllowNull = true ; ClrType = typeof<string> ; SqlType = DbType.String }
                { Name = "Surname"   ; SqlTypeAsString = "nvarchar(max)"; AllowNull = false; ClrType = typeof<string> ; SqlType = DbType.String }
            ]

            Expect.equal metadata.Name "#Persons" "Wrong name of temporary table"
            Expect.sequenceEqual metadata.Columns expectedColumns "Wrong columns metadata"
        }
    ]