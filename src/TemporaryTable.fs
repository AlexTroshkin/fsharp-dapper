namespace FSharp.Data.Dapper

open System
open System.Collections
open System.Data
open System.Reflection

[<AutoOpen>]
module TemporaryTable =

    type Table = 
        { Name : string 
          Rows : IEnumerable }

    type ColumnMetadata =
        { Name            : string 
          ClrType         : Type
          SqlType         : DbType
          SqlTypeAsString : string
          AllowNull       : bool }

    type TableMetadata = 
        { Name    : string
          Columns : ColumnMetadata list }

    module Metadata =
    
        let private CreateColumnsMetadata (tableType : Type) =
            let properties = tableType.GetProperties(BindingFlags.Instance ||| BindingFlags.Public ||| BindingFlags.GetProperty)        
            properties
            |> Array.map (fun property ->
                let underlyingType = Reflection.TryGetUnderlyingType property.PropertyType
                let columnName = property.Name
                let allowNull  = if underlyingType.IsSome then true else false
                let typeMapping =
                    let clrColumnType = 
                        match  underlyingType with
                        | Some underlyingType -> underlyingType
                        | None -> property.PropertyType

                    match SqlTypeMapping.tryFind clrColumnType with
                    | Some mapping -> mapping
                    | None -> failwith (sprintf "Can't find sql type mapping for %s" clrColumnType.FullName)

                { Name            = columnName
                  ClrType         = typeMapping.CLR
                  SqlType         = typeMapping.SQL
                  SqlTypeAsString = typeMapping.SQL_Name
                  AllowNull       = allowNull })

            |> List.ofArray

        let Create (table : Table) = 
            let name = sprintf "#%s" table.Name
            let tableType = 
                match Reflection.TryGetTypeOfSeq table.Rows with
                    | None         -> failwith "Can't datermine type for temporary table: Collection is empty and not generic type definition"
                    | Some clrType -> clrType

            let columns = CreateColumnsMetadata tableType

            { Name = name; Columns = columns }

    module Data =

        let Create table metadata =
            let generatedTable = new DataTable(metadata.Name)

            metadata.Columns
            |> Array.ofList
            |> Array.map (fun col -> new DataColumn (col.Name, col.ClrType))
            |> (fun cols -> generatedTable.Columns.AddRange(cols))

            table.Rows
            |> Seq.cast<obj>
            |> Seq.iter (fun rowData -> 
                let row = 
                    metadata.Columns 
                    |> Array.ofList
                    |> Array.map (fun col -> 
                        let value = rowData.GetType().GetProperty(col.Name).GetValue(rowData)
                        if Object.ReferenceEquals(null, value) 
                            then (DBNull.Value :> obj)
                            else value 
                    )
                generatedTable.Rows.Add(row) |> ignore
            )

            generatedTable

    module Sql =

        let CreateScript metadata =
            sprintf "create table %s (%s)"
                metadata.Name 
                (metadata.Columns
                    |> List.map (fun col -> sprintf "%s %s %s" col.Name col.SqlTypeAsString (if col.AllowNull then "null" else "not null"))
                    |> (fun columns -> String.Join(",", columns)))

        let DropScript metadata =
            sprintf "drop table %s" metadata.Name