namespace FSharp.Data.Dapper

open System.Collections

[<AutoOpen>]
module TemporaryTableHelpers =
    let DatermineTableType (rows : IEnumerable) =
        match rows |> Seq.cast<obj> |> Seq.tryHead with
        | Some head -> head.GetType()
        | None -> 
            let tableType = rows.GetType()
            match tableType.IsGenericTypeDefinition with
            | false -> failwith "Can't datermine type for temporary table: Collection is empty and not generic type definition"
            | true  -> tableType.GetGenericArguments().[0]

[<AutoOpen>]
module TemporaryTable =

    type TableDefinition = 
        { Name : string 
          Rows : IEnumerable }
        
    
    let private CreateTableName tableDefinition = sprintf "#%s" tableDefinition.Name
    let private CreateTableDefinition tableName columnsDefinition = sprintf "%s (%s)" tableName columnsDefinition
    let private CreateColumnsDefinition tableDefinition =

        ""

    let CreateSqlTableDefinition tableDefinition =

        let columnsDefinition = tableDefinition |> CreateColumnsDefinition 
        let tableName = tableDefinition |> CreateTableName 
        let tableDefinition = CreateTableDefinition tableName columnsDefinition

        sprintf "create table %s" tableDefinition