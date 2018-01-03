module TemporaryTableDataTests

open Expecto
open InMemoryDatabase.Types
open FSharp.Data.Dapper.TemporaryTable
open System.Data
open System

[<Tests>]
let tests =
    testList "temporary table data tests" [
        test "Verify person temporary table data" {
            let rows = [{ Id = 1L; Name = "Ivan" ; Patronymic = None; Surname = "Ivanov" }]
            let table = { Name = "Persons"; Rows = rows }
            let metadata = Metadata.Create table

            let expectedColumns = 
                [ ("Id"        , typedefof<int64> )
                  ("Name"      , typedefof<string>)
                  ("Patronymic", typedefof<string>)
                  ("Surname"   , typedefof<string>) ] |> Seq.ofList

            let expectedRows = [
                [| 1L :> obj; "Ivan" :> obj; DBNull.Value :> obj; "Ivanov" :> obj |]
            ]

            let dataTable = Data.Create table metadata
            let actualColumns = 
                dataTable.Columns 
                |> Seq.cast<DataColumn> 
                |> Seq.map (fun col -> (col.ColumnName, col.DataType))

            let actualRows = 
                dataTable.Rows
                |> Seq.cast<DataRow>
                |> Seq.map (fun row -> row.ItemArray)

            Expect.sequenceEqual actualColumns expectedColumns "Wrong columns in data table"
            Expect.sequenceEqual actualRows expectedRows "Wrong rows in data table"
        }
    ]