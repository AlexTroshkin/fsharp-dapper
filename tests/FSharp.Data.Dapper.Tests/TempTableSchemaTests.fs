module TempTableSchemaTests

open Expecto

open InMemoryDatabase.Types

open System.Data

open FSharp.Data.Dapper
open FSharp.Data.Dapper.Enums
open FSharp.Data.Dapper.TempTable
open FSharp.Data.Dapper.TempTable.Schema
open FSharp.Data.Dapper
open FSharp.Data.Dapper

[<Tests>]
let tests = 
    testList "Temp table schema tests" [
        test "Verify schema of person temp table" {
            let rows = [{ Id = 1L; Name = "" ; Patronymic = None; Surname = "" }]

            let schemaOfTable = Schema.Create Sqlite "TPersons" rows
            let expectedColumns = [
                { Name = "Id"
                  AllowNull = false
                  TypeCorellation = 
                    { ClrTypeRepresentation = typeof<int64>
                      SqlTypeRepresentation = SqlTypeRepresentation(SqlDbType.BigInt, "BigInt") }}

                { Name = "Name"
                  AllowNull = false
                  TypeCorellation = 
                    { ClrTypeRepresentation = typeof<string>
                      SqlTypeRepresentation = SqlTypeRepresentation(SqlDbType.NVarChar, "NVarChar", ["Max"]) }}

                { Name = "Patronymic"
                  AllowNull = true
                  TypeCorellation = 
                    { ClrTypeRepresentation = typeof<string>
                      SqlTypeRepresentation = SqlTypeRepresentation(SqlDbType.NVarChar, "NVarChar", ["Max"]) }}

                { Name = "Surname"
                  AllowNull = false
                  TypeCorellation = 
                    { ClrTypeRepresentation = typeof<string>
                      SqlTypeRepresentation = SqlTypeRepresentation(SqlDbType.NVarChar, "NVarChar", ["Max"]) }}
            ]

            Expect.equal schemaOfTable.Name "TPersons" "Wrong name of temp table"
            Expect.sequenceEqual schemaOfTable.Columns expectedColumns "Wrong schema of temp table columns"
        }

        test "Verify schema of single column temp table" {
            let identificators = seq { 1 .. 5 }

            let schemaOfTable = Schema.CreateWithSingleColumn Sqlite "PersonIdentificators" "Id" identificators
            let expectedColumn = 
                { Name            = "Id" 
                  AllowNull       = false
                  TypeCorellation =
                    { ClrTypeRepresentation = typeof<int>
                      SqlTypeRepresentation = SqlTypeRepresentation (SqlDbType.Int, "Int") }}

            Expect.equal schemaOfTable.Name "PersonIdentificators" "Wrong name of single column temp table"
            Expect.equal schemaOfTable.Columns.[0] expectedColumn "Worng schema of single column temp table"
        }
    ]