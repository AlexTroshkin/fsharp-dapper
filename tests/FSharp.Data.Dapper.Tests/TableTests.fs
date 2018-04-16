module TableTests

open Expecto
open FSharp.Data.Dapper
open FSharp.Data.Dapper.Table
open FSharp.Data.Dapper.Table.Schema
open FSharp.Data.Dapper.Table.Scripts

open SqliteDatabase.Types

let dummyConnection = SqliteConnection (null)

module ExpectedSchemes =
    
    let private int64TypeMapping  = TypeMapping.Find dummyConnection typeof<int64>
    let private stringTypeMapping = TypeMapping.Find dummyConnection typeof<string>

    let Person = 
        { Name = "TPerson"
          Columns = 
          [|
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
        }

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
            let persons = [{ Id = 1L; Name = "" ; Patronymic = None; Surname = "" }]
            let actualSchema  = Schema.Create dummyConnection "TPerson" persons

            Expect.equal actualSchema ExpectedSchemes.Person "Wrong scheme"
        }
    ]

[<Tests>]
let scriptsTests = 
    testList "Table -> Scripts" [

        test "Check scripts" {
            let expectedScripts = 
                { Create = "CREATE TEMP TABLE TPerson (Id BigInt NOT NULL,Name NText NOT NULL,Patronymic NText NULL,Surname NText NOT NULL)"
                  Drop   = "DROP TABLE TPerson" }

            let actualScripts = Scripts.Create dummyConnection ExpectedSchemes.Person            

            Expect.equal actualScripts expectedScripts "Wrong scripts"
        }

    ]