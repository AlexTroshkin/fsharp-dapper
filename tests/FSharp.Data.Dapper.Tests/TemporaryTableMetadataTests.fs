module TemporaryTableMetadataTests

open Expecto

open InMemoryDatabase.Types

open FSharp.Data.Dapper
open FSharp.Data.Dapper.TemporaryTable

[<Tests>]
let tests = 
    testList "Temporary table metadata tests" [
        test "Verify person temporary table metadata" {
            let rows = [{ Id = 1L; Name = "" ; Patronymic = None; Surname = "" }]
            let table = { Name = "Persons"; Rows = rows }

            let metadata = Metadata.Create table
            let expectedColumns = [
                { Name = "Id"        ; SqlType = "bigint"       ; AllowNull = false }
                { Name = "Name"      ; SqlType = "nvarchar(max)"; AllowNull = false }
                { Name = "Patronymic"; SqlType = "nvarchar(max)"; AllowNull = true  }
                { Name = "Surname"   ; SqlType = "nvarchar(max)"; AllowNull = false }
            ]

            Expect.equal metadata.Name "#Persons" "Wrong name of temporary table"
            Expect.sequenceEqual metadata.Columns expectedColumns "Wrong columns metadata"
        }
    ]