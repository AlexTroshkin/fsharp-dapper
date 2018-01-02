namespace FSharp.Data.Dapper

open System
open System.Collections
open System.Reflection

module TemporaryTableReflection =

    let DatermineTableType (rows : IEnumerable) =
        match rows |> Seq.cast<obj> |> Seq.tryHead with
        | Some head -> Some (head.GetType())
        | None -> 
            let tableType = rows.GetType()
            match tableType.IsGenericTypeDefinition with
            | false -> None 
            | true  -> tableType.GenericTypeArguments |> Array.tryHead
    
    let TryGetUnderlyingType (clrType : Type) = 
        let hasUnderlyingType = clrType.IsGenericType && (clrType = typedefof<option<_>> || clrType = typedefof<Nullable<_>>)
        
        match hasUnderlyingType with
        | false -> None
        | true  -> clrType.GenericTypeArguments |> Array.tryHead

[<AutoOpen>]
module TemporaryTable =

    type Table = 
        { Name : string 
          Rows : IEnumerable }
          
    type TableColumnMetadata =
        { Name      : string 
          SqlType   : string
          AllowNull : bool }
    
    type TableMetadata = 
        { Name    : string
          Columns : TableColumnMetadata list }

    module Metadata =
    
        let private CreateColumnsMetadata table =
            let tableType = TemporaryTableReflection.DatermineTableType table.Rows
            let properties = 
                match tableType with
                | Some clrType -> clrType.GetProperties(BindingFlags.Instance ||| BindingFlags.Public ||| BindingFlags.GetProperty)
                | None         -> failwith "Can't datermine type for temporary table: Collection is empty and not generic type definition"
        
            properties
            |> Array.map (fun property ->
                let underlyingType = TemporaryTableReflection.TryGetUnderlyingType property.PropertyType
                let columnName = property.Name
                let allowNull  = if underlyingType.IsSome then true else false
                let columnType =
                    let clrType =
                        match  underlyingType with
                        | Some underlyingType -> underlyingType
                        | None -> property.PropertyType
                
                    match SqlTypeMapping.tryFind clrType with
                    | Some mapping -> mapping.SQL_Name
                    | None -> failwith (sprintf "Can't find sql type mapping for %s" clrType.FullName)

            
                { Name = columnName; SqlType = columnType; AllowNull = allowNull })
            |> List.ofArray

        let Create (table : Table) = 
            let name = sprintf "#%s" table.Name
            let columns = CreateColumnsMetadata table

            { Name = name; Columns = columns }