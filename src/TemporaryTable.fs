namespace FSharp.Data.Dapper

open System
open System.Collections
open System.Data
open System.Reflection

type TemporaryTable = 
    { Data      : DataTable
      SqlCreate : string 
      SqlDrop   : string }

[<AutoOpen>]
module TemporaryTable =
    
    module Metadata =

        type Column =
            { Name            : string 
              ClrType         : Type
              SqlType         : DbType
              SqlTypeAsString : string
              AllowNull       : bool }

        type TemporaryTable = 
            { Name    : string
              Columns : Column list }
    
        let private findMapping originalType underlyingType =
            let columnType = underlyingType |> Option.defaultValue originalType
            let mapping = SqlTypeMapping.tryFind columnType  
            
            match mapping with
            | None -> failwith (sprintf "Can't find sql type mapping for %s" columnType.FullName)
            | Some mapping -> mapping

        let private CreateColumn 
            (nameOfColumn : string) 
            (typeOfColumn : Type) =

            let underlyingType = Reflection.TryGetUnderlyingType typeOfColumn
            let typeMapping    = findMapping typeOfColumn underlyingType
            let allowNull      = underlyingType.IsSome

            { Name            = nameOfColumn
              ClrType         = typeMapping.CLR
              SqlType         = typeMapping.SQL
              SqlTypeAsString = typeMapping.SQL_Name
              AllowNull       = allowNull }

        let private CreateColumns (typeOfTable : Type) =
            typeOfTable.GetProperties(BindingFlags.Instance ||| BindingFlags.Public ||| BindingFlags.GetProperty) 
            |> Array.map (fun property -> CreateColumn property.Name property.PropertyType)
            |> List.ofArray        

        let private formatNameOfTable nameOfTable = sprintf "#%s" nameOfTable
        let private getTypeOfSeq sequence = 
            match Reflection.TryGetTypeOfSeq sequence with
            | None   -> failwith "Can't datermine type for temporary table: Collection is empty and not generic type definition"
            | Some t -> t

        let Create nameOfTable rowsOfTable = 
            let typeOfTable = getTypeOfSeq rowsOfTable

            { Name    = formatNameOfTable nameOfTable 
              Columns = CreateColumns typeOfTable }

        let CreateWithSingleColumn 
            nameOfTable
            nameOfColumn
            rowsOfTable =

            let typeOfColumn = getTypeOfSeq rowsOfTable
 
            { Name    = formatNameOfTable nameOfTable 
              Columns = [CreateColumn nameOfColumn typeOfColumn] }
        
    module Data =

        let Create rowsOfTable (metadataOfTable : Metadata.TemporaryTable) =
            let generatedTable = new DataTable(metadataOfTable.Name)

            metadataOfTable.Columns
            |> Array.ofList
            |> Array.map (fun col -> new DataColumn (col.Name, col.ClrType))
            |> (fun cols -> generatedTable.Columns.AddRange(cols))

            for row in rowsOfTable do
                let rowItems = 
                    metadataOfTable.Columns 
                    |> Array.ofList
                    |> Array.map (fun col -> 
                        let value = row.GetType().GetProperty(col.Name).GetValue(row)
                        if Object.ReferenceEquals(null, value)
                            then (DBNull.Value :> obj)
                            else value)

                generatedTable.Rows.Add(rowItems) |> ignore
            
            generatedTable

        let CreateWithSingleColumn 
            (rowsOfTable     : IEnumerable)
            (metadataOfTable : Metadata.TemporaryTable) =

            let generatedTable = new DataTable(metadataOfTable.Name)
            let metadataOfColumn = metadataOfTable.Columns.[0]
            let column = new DataColumn(metadataOfColumn.Name, metadataOfColumn.ClrType)

            generatedTable.Columns.Add(column)

            for row in rowsOfTable do
                generatedTable.Rows.Add ([|row|]) |> ignore

            generatedTable

    module Sql =

        let CreateScript (metadataOfTable : Metadata.TemporaryTable) =
            sprintf "create table %s (%s)"
                metadataOfTable.Name 
                (metadataOfTable.Columns
                    |> List.map (fun col -> sprintf "%s %s %s" col.Name col.SqlTypeAsString (if col.AllowNull then "null" else "not null"))
                    |> (fun columns -> String.Join(",", columns)))

        let DropScript (metadataOfTable : Metadata.TemporaryTable) =
            sprintf "drop table %s" metadataOfTable.Name


    let Create nameOfTable rowsOfTable =
        let metadataOfTable = Metadata.Create nameOfTable rowsOfTable
        let dataOfTable     = Data.Create rowsOfTable metadataOfTable

        { Data      = dataOfTable
          SqlCreate = Sql.CreateScript metadataOfTable
          SqlDrop   = Sql.DropScript metadataOfTable }

    let CreateWithSingleColumn
        nameOfTable
        nameOfColumn
        rowsOfTable =

        let metadataOfTable = Metadata.CreateWithSingleColumn nameOfTable nameOfColumn rowsOfTable
        let dataOfTable     = Data.CreateWithSingleColumn rowsOfTable metadataOfTable

        { Data      = dataOfTable
          SqlCreate = Sql.CreateScript metadataOfTable
          SqlDrop   = Sql.DropScript metadataOfTable }