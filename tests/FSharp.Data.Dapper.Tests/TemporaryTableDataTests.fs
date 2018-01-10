module TemporaryTableDataTests

open Expecto
open InMemoryDatabase.Types
open FSharp.Data.Dapper.TemporaryTable
open System.Data
open System

[<Tests>]
let tests =
    testList "Temp table data tests" [
        test "Verify data of person temp table" {
            let rows = [{ Id = 1L; Name = "Ivan" ; Patronymic = None; Surname = "Ivanov" }]
            let metadata = Metadata.Create "Persons" rows

            let expectedColumns = 
                [ ("Id"        , typedefof<int64> )
                  ("Name"      , typedefof<string>)
                  ("Patronymic", typedefof<string>)
                  ("Surname"   , typedefof<string>) ] |> Seq.ofList

            let expectedRows = [
                [| box 1L; box "Ivan"; box DBNull.Value; box "Ivanov" |]
            ]

            let dataTable = Data.Create rows metadata
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

        test "Verify data of single column temp table" {
            let expectedNameOfColumn = "Id"
            let expectedTypeOfColumn = typedefof<int32>

            let identificators = seq { 1 .. 5 }
            let metadataOfTable = Metadata.CreateWithSingleColumn "PersonIdentificators" "Id" identificators

            let dataOfTable = Data.CreateWithSingleColumn identificators metadataOfTable
            let actualColumn = dataOfTable.Columns.[0]
            let actualRows = 
                dataOfTable.Rows 
                |> Seq.cast<DataRow>
                |> Seq.map (fun row -> row.["Id"] :?> int)
            
            Expect.equal actualColumn.ColumnName expectedNameOfColumn "Wrong name of column"
            Expect.equal actualColumn.DataType expectedTypeOfColumn "Wrong type of column"
            Expect.sequenceEqual actualRows identificators "Wrong rows in data table"
        }
    ]