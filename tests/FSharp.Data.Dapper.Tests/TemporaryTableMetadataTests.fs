module TemporaryTableMetadataTests

open Expecto

open InMemoryDatabase.Types

open System.Data

open FSharp.Data.Dapper
open FSharp.Data.Dapper.TemporaryTable
open FSharp.Data.Dapper.TemporaryTable.Metadata

[<Tests>]
let tests = 
    testList "Temp table metadata tests" [
        test "Verify metadata of person temp table" {
            let rows = [{ Id = 1L; Name = "" ; Patronymic = None; Surname = "" }]

            let metadata = Metadata.Create "Persons" rows
            let expectedColumns = [
                { Name = "Id"        ; SqlTypeAsString = "bigint"       ; AllowNull = false; ClrType = typeof<int64>  ; SqlType = DbType.Int64 }
                { Name = "Name"      ; SqlTypeAsString = "nvarchar(max)"; AllowNull = false; ClrType = typeof<string> ; SqlType = DbType.String }
                { Name = "Patronymic"; SqlTypeAsString = "nvarchar(max)"; AllowNull = true ; ClrType = typeof<string> ; SqlType = DbType.String }
                { Name = "Surname"   ; SqlTypeAsString = "nvarchar(max)"; AllowNull = false; ClrType = typeof<string> ; SqlType = DbType.String }
            ]

            Expect.equal metadata.Name "#Persons" "Wrong name of temp table"
            Expect.sequenceEqual metadata.Columns expectedColumns "Wrong metadata of temp table columns"
        }

        test "Verify metadata of single column temp table" {
            let identificators = seq { 1 .. 5 }

            let metadataOfTable = Metadata.CreateWithSingleColumn "PersonIdentificators" "Id" identificators
            let expectedColumn = 
                { Name            = "Id" 
                  SqlTypeAsString = "int"
                  AllowNull       = false
                  ClrType         = typeof<int>  
                  SqlType         = DbType.Int32 }

            Expect.equal metadataOfTable.Name "#PersonIdentificators" "Wrong name of single column temp table"
            Expect.equal metadataOfTable.Columns.[0] expectedColumn "Worng metadata of single column temp table"
        }
    ]