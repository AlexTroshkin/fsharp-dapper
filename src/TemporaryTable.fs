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

    type TableDefinition = 
        { Name : string 
          Rows : IEnumerable }
    
    let private CreateTableName tableDefinition = sprintf "#%s" tableDefinition.Name
    let private CreateTableDefinition tableName columnsDefinition = sprintf "%s (%s)" tableName columnsDefinition
    let private CreateColumnsDefinition tableDefinition =
        let tableType = TemporaryTableReflection.DatermineTableType tableDefinition.Rows
        let properties = 
            match tableType with
            | Some clrType -> clrType.GetProperties(BindingFlags.Instance ||| BindingFlags.Public)
            | None         -> failwith "Can't datermine type for temporary table: Collection is empty and not generic type definition"
        
        properties 
        |> Array.map (fun property ->
            let underlyingType = TemporaryTableReflection.TryGetUnderlyingType property.DeclaringType

            let columnName = property.Name
            let columnType =
                let clrType =
                    match  underlyingType with
                    | Some underlyingType -> underlyingType
                    | None -> property.DeclaringType
                
                match SqlTypeMapping.tryFind clrType with
                | Some mapping -> mapping.SQL_Name
                | None -> failwith ""

            let nullModifier = 
                match underlyingType with
                | Some _ -> "null"
                | None   -> "not null"
            
            sprintf "%s %s %s" columnName columnType nullModifier)

        |> (fun columnsDefinition -> String.Join(",", columnsDefinition))

    let CreateSqlTableDefinition tableDefinition =
    
        let columnsDefinition = tableDefinition |> CreateColumnsDefinition 
        let tableName = tableDefinition |> CreateTableName 
        let tableDefinition = CreateTableDefinition tableName columnsDefinition

        sprintf "create table %s" tableDefinition