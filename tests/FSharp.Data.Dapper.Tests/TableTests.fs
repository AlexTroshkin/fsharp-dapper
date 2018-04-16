module TableTests

open Expecto
open FSharp.Data.Dapper
open FSharp.Data.Dapper.Table
open FSharp.Data.Dapper.Table.Schema
open SqliteDatabase.Types

let dummyConnection = SqliteConnection (null)

[<Tests>]
let schemaTests = 
    testList "Table -> Schema" [

        test "Check the scheme for a sequence of primitives" {
            let schema = Schema.Create dummyConnection "PersonID" (seq { 1 .. 5 })
            
            let expectedName = "PersonID" 
            let expectedColumns = [|
                { Name      = "Value"
                  AllowNull = false
                  Type      = TypeMapping.Find dummyConnection typeof<int> }
            |]

            Expect.equal schema.Name    expectedName    "Wrong name of table"
            Expect.equal schema.Columns expectedColumns "Wrong columns of table"
        }

        test "Check schema for 'TPerson' table" {
            let int64TypeMapping  = TypeMapping.Find dummyConnection typeof<int64>
            let stringTypeMapping = TypeMapping.Find dummyConnection typeof<string>

            let expectedName = "TPerson" 
            let expectedColumns = [|
                { Name      = "Id"
                  AllowNull = false
                  Type      = int64TypeMapping }

                { Name      = "Name"
                  AllowNull = false
                  Type      = stringTypeMapping }

                { Name      = "Patronymic"
                  AllowNull = true
                  Type      = stringTypeMapping }

                { Name      = "Surname"
                  AllowNull = false
                  Type      = stringTypeMapping }
            |]

            let persons = [{ Id = 1L; Name = "" ; Patronymic = None; Surname = "" }]
            let actualSchema  = Schema.Create dummyConnection "TPerson" persons

            Expect.equal actualSchema.Name    expectedName    "Wrong name of table"
            Expect.equal actualSchema.Columns expectedColumns "Wrong columns of table"
        }
    ]