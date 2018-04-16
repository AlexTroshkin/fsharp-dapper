module TableTests

open Expecto
open FSharp.Data.Dapper
open FSharp.Data.Dapper.Table
open FSharp.Data.Dapper.Table.Schema


let dummyConnection = SqliteConnection (null)

[<Tests>]
let schemaTests = 
    testList "Table -> Schema" [
        test "Check the scheme for a sequence of primitives " {
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
    ]