namespace FSharp.Data.Dapper

open System
open System.Data
open System.Data.SqlClient
open Dapper

[<AutoOpen>]
module Query =
    
    type QueryDefinition = 
        { Script          : string
          Parameters      : obj option
          TemporaryTables : Table list option }
    
    let DefaultQueryDefinition = 
        { Script          = String.Empty
          Parameters      = None
          TemporaryTables = None }

    let private await task = task |> Async.AwaitTask
    let private parametersOf queryDefinition =
        match queryDefinition.Parameters with
        | Some p -> p
        | None -> null

    let private UseTemporaryTables 
        (connection : IDbConnection)
        (queryDefinition : QueryDefinition) = async {

        let createAndCopy table = async {
            let! _ = await <| connection.ExecuteAsync(table.SqlCreate)
            use  bulkCopy = new SqlBulkCopy(connection :?> SqlConnection)

            bulkCopy.DestinationTableName <- table.Data.TableName
            do! bulkCopy.WriteToServerAsync(table.Data) |> Async.AwaitTask

            return ()
        }

        match queryDefinition.TemporaryTables with
        | None -> ()
        | Some tables ->
            connection.Open()
            tables |> List.map createAndCopy
                   |> Async.Parallel
                   |> Async.RunSynchronously
                   |> ignore
    }

    let private ReleaseTemporaryTables 
        (connection : IDbConnection)
        (queryDefinition : QueryDefinition) = async {

        match queryDefinition.TemporaryTables with
        | None -> ()
        | Some tables -> tables |> List.map (fun table -> table.SqlDrop)
                                |> (fun dropScripts -> String.Join("\n", dropScripts))
                                |> (fun dropScript  -> connection.ExecuteAsync(dropScript))
                                |> Async.AwaitTask
                                |> Async.RunSynchronously
                                |> ignore
    }

    let QueryAsync<'TRow> 
        (queryDefinition : QueryDefinition) 
        (connection : IDbConnection) = async {

        do!  UseTemporaryTables connection queryDefinition
        let! queryResult = await <| connection.QueryAsync<'TRow>(queryDefinition.Script, parametersOf queryDefinition)
        do!  ReleaseTemporaryTables connection queryDefinition

        return queryResult
    }

    let QuerySingleAsync<'T>
        (queryDefinition : QueryDefinition)
        (connection : IDbConnection) = async {

        do!  UseTemporaryTables connection queryDefinition
        let! single = await <| connection.QuerySingleOrDefaultAsync<'T>(queryDefinition.Script, parametersOf queryDefinition)
        do!  ReleaseTemporaryTables connection queryDefinition

        return 
            if Object.ReferenceEquals(null, single) 
                then None
                else Some single 
    }

    let ExecuteAsync 
        (queryDefinition : QueryDefinition)
        (connection : IDbConnection) = async {

        do!  UseTemporaryTables connection queryDefinition
        let! countOfAffectedRows = await <| connection.ExecuteAsync(queryDefinition.Script, parametersOf queryDefinition)
        do!  ReleaseTemporaryTables connection queryDefinition

        return countOfAffectedRows
    }