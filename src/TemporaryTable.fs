namespace FSharp.Data.Dapper

open System
open System.Collections
open System.Data
open System.Reflection

type TempTable =
    | ``Temp table`` of
        ``name of table``  : string *
        ``rows``           : IEnumerable
    | ``Temp table with one column`` of
        ``name of table``  : string *
        ``name of column`` : string *
        ``rows``           : IEnumerable

type GeneratedTempTable = 
    { Data      : DataTable
      SqlCreate : string 
      SqlDrop   : string }

[<AutoOpen>]
module TempTable =
    open Enums

    module Schema =
        open Enums

        type Column =
            { Name            : string 
              TypeCorellation : SqlClrTypeCorellation
              AllowNull       : bool }

        type TempTable = 
            { Name    : string
              Columns : Column list }
    
        let private findCorellation dbType originalType underlyingType =
            let columnType = underlyingType |> Option.defaultValue originalType
            let corellation = SqlClrTypeCorellation.TryFindCorellation dbType columnType  
            
            match corellation with
            | None -> failwith (sprintf "Can't find type corellation for %s" columnType.FullName)
            | Some value -> value

        let private CreateColumn
            (dbType       : DatabaseType)
            (nameOfColumn : string) 
            (typeOfColumn : Type) =

            let underlyingType = Reflection.TryGetUnderlyingType typeOfColumn
            let corellation    = findCorellation dbType typeOfColumn underlyingType
            let allowNull      = underlyingType.IsSome

            { Name            = nameOfColumn
              TypeCorellation = corellation
              AllowNull       = allowNull }

        let private CreateColumns 
            (dbType      : DatabaseType)
            (typeOfTable : Type) =
        
            typeOfTable.GetProperties(BindingFlags.Instance ||| BindingFlags.Public ||| BindingFlags.GetProperty) 
            |> Array.map (fun property -> CreateColumn dbType property.Name property.PropertyType)
            |> List.ofArray        

        let private formatNameOfTable dbType nameOfTable = 
            match dbType with
            | SqlServer -> sprintf "#%s" nameOfTable
            | Sqlite    -> nameOfTable
            | _         -> failwith (sprintf "Unsupported data base %A" dbType)

        let private getTypeOfSeq sequence = 
            match Reflection.TryGetTypeOfSeq sequence with
            | None   -> failwith "Can't datermine type for temporary table: Collection is empty and not generic type definition"
            | Some t -> t

        let Create dbType nameOfTable rowsOfTable = 
            let typeOfTable = getTypeOfSeq rowsOfTable

            { Name    = formatNameOfTable dbType nameOfTable 
              Columns = CreateColumns dbType typeOfTable }

        let CreateWithSingleColumn
            dbType
            nameOfTable
            nameOfColumn
            rowsOfTable =

            let typeOfColumn = getTypeOfSeq rowsOfTable
 
            { Name    = formatNameOfTable dbType nameOfTable 
              Columns = [CreateColumn dbType nameOfColumn typeOfColumn] }

    module Data =

        let Create (rowsOfTable : IEnumerable) (metadataOfTable : Schema.TempTable) =
            let generatedTable = new DataTable(metadataOfTable.Name)

            metadataOfTable.Columns
            |> Array.ofList
            |> Array.map (fun col -> new DataColumn (col.Name, col.TypeCorellation.ClrTypeRepresentation))
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
            (schemaOfTable   : Schema.TempTable) =

            let generatedTable = new DataTable(schemaOfTable.Name)
            let schemaOfColumn = schemaOfTable.Columns.[0]
            let column = new DataColumn(schemaOfColumn.Name, schemaOfColumn.TypeCorellation.ClrTypeRepresentation)

            generatedTable.Columns.Add(column)

            for row in rowsOfTable do
                generatedTable.Rows.Add ([|row|]) |> ignore

            generatedTable

    module Sql =

        let private schemaOfColumnToSql (schemaOfColumn : Schema.Column) =
            sprintf "%s %s %s"
                schemaOfColumn.Name 
                schemaOfColumn.TypeCorellation.SqlTypeRepresentation.AsStringWithParaeters 
                (if schemaOfColumn .AllowNull then "null" else "not null")

        let CreateScript 
            (dbType        : DatabaseType)
            (schemaOfTable : Schema.TempTable) =
            
            let columns = 
                schemaOfTable.Columns
                |> List.map schemaOfColumnToSql
                |> String.concat ","

            match dbType with
            | SqlServer -> sprintf "create table %s (%s)" schemaOfTable.Name columns
            | Sqlite    -> sprintf "create temp table %s (%s)" schemaOfTable.Name columns

        let DropScript (schemaOfTable : Schema.TempTable) =
            sprintf "drop table %s" schemaOfTable.Name

    let private create 
        (dbType      : DatabaseType)
        (nameOfTable : string)
        (rowsOfTable : IEnumerable) =
        
        let schemaOfTable = Schema.Create dbType nameOfTable rowsOfTable
        let dataOfTable   = Data.Create rowsOfTable schemaOfTable

        { Data      = dataOfTable
          SqlCreate = Sql.CreateScript dbType schemaOfTable
          SqlDrop   = Sql.DropScript schemaOfTable }

    let private createWithSingleColumn
        dbType
        nameOfTable
        nameOfColumn
        rowsOfTable =

        let schemaOfTable = Schema.CreateWithSingleColumn dbType nameOfTable nameOfColumn rowsOfTable
        let dataOfTable   = Data.CreateWithSingleColumn rowsOfTable schemaOfTable

        { Data      = dataOfTable
          SqlCreate = Sql.CreateScript dbType schemaOfTable
          SqlDrop   = Sql.DropScript schemaOfTable }

    let Create tempTable dbType =
        match tempTable with
        | ``Temp table`` (name, rows) -> create dbType name rows
        | ``Temp table with one column`` (name, column, rows) -> createWithSingleColumn dbType name column rows