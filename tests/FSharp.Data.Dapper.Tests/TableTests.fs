module TableTests

open Expecto
open FSharp.Data.Dapper
open FSharp.Data.Dapper.Table
open FSharp.Data.Dapper.Table.Scheme
open FSharp.Data.Dapper.Table.Scripts

open SqliteDatabase.Types
open System
open System.Data

let dummyConnection = SqliteConnection (null)

module ExpectedSchemes =
    
    let private int64TypeMapping  = TypeMapping.Find dummyConnection typeof<int64>
    let private stringTypeMapping = TypeMapping.Find dummyConnection typeof<string>

    let Person = 
        { Name    = "TPerson"
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

    let PersonID =
        { Name    = "PersonID" 
          Columns = 
          [|
            { Name      = "Value"
              AllowNull = false
              Type      = TypeMapping.Find dummyConnection typeof<int> }          
          |] 
        }

[<Tests>]
let schemeTests = 
    testList "Table -> Scheme" [

        test "Check the scheme for a sequence of primitives" {
            let scheme = Scheme.Create dummyConnection "PersonID" (seq { 1 .. 5 })
            
            let expectedName = "PersonID" 
            let expectedColumns = [|
                { Name      = "Value"
                  AllowNull = false
                  Type      = TypeMapping.Find dummyConnection typeof<int> }
            |]

            Expect.equal scheme.Name    expectedName    "Wrong name of table"
            Expect.equal scheme.Columns expectedColumns "Wrong columns of table"
        }

        test "Check scheme for 'TPerson' table" {
            let persons = [{ Id = 1L; Name = "" ; Patronymic = None; Surname = "" }]
            let actualScheme  = Scheme.Create dummyConnection "TPerson" persons

            Expect.equal actualScheme ExpectedSchemes.Person "Wrong scheme"
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

[<Tests>]
let dataTests = 
    testList "Table -> Data" [
        test "Check DataTable for 'Person' table" {
            let expectedColumns = 
                [ ("Id"        , typedefof<int64> )
                  ("Name"      , typedefof<string>)
                  ("Patronymic", typedefof<string>)
                  ("Surname"   , typedefof<string>) ] |> Seq.ofList

            let expectedRows = [
                [| box 1L; box "Ivan"; box DBNull.Value; box "Ivanov" |]
            ]

            let rows = [{ Id = 1L; Name = "Ivan" ; Patronymic = None; Surname = "Ivanov" }]
            let data = Data.Create ExpectedSchemes.Person rows

            let actualColumns = 
                data.Columns |> Seq.cast<DataColumn>
                             |> Seq.map (fun c -> (c.ColumnName, c.DataType))

            let actualRows =
                data.Rows |> Seq.cast<DataRow>
                          |> Seq.map (fun r -> r.ItemArray)                 

            Expect.sequenceEqual actualColumns expectedColumns "Wrong data table columns"
            Expect.sequenceEqual actualRows    expectedRows    "Wrong data table rows"
        }

        test "Check DataTable for 'PersonID' table" {
            let expectedColumn = ("Value", typedefof<int32>)
            let expectedRows = seq { 1 .. 10 } 

            let data = Data.Create ExpectedSchemes.PersonID expectedRows 

            let actualColumn = (data.Columns.[0].ColumnName, data.Columns.[0].DataType)
            let actualRows =
                data.Rows |> Seq.cast<DataRow>
                          |> Seq.map (fun row -> row.["Value"] :?> int)

            Expect.equal         actualColumn expectedColumn  "Wrong data table column"
            Expect.sequenceEqual actualRows   expectedRows    "Wrong data table rows"
        }
    ]