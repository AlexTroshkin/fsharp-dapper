namespace FSharp.Data.Dapper

open System
open System.Data
open System.Data.SqlClient
open Dapper

type Query (script : string, ?parameters : obj, ?temporaryTables : GeneratedTempTable list) =
    member __.Script = script
    member __.Parameters = parameters
    member __.TemporaryTables = temporaryTables

[<AutoOpen>]
module Query =
    
    module Parameters =
        let (<=>) (key:string) value = key, box value
        let Create (parameters : list<string * obj>) = dict(parameters) :> obj

    let private await task = task |> Async.AwaitTask
    let private parametersOf (query : Query) =
        match query.Parameters with
        | Some p -> p
        | None -> null

    let private UseTemporaryTables 
        (connection : IDbConnection)
        (query : Query) = async {

        let createAndCopy table = async {
            let! _ = await <| connection.ExecuteAsync(table.SqlCreate)
            use  bulkCopy = new SqlBulkCopy(connection :?> SqlConnection)

            bulkCopy.DestinationTableName <- table.Data.TableName
            do! bulkCopy.WriteToServerAsync(table.Data) |> Async.AwaitTask

            return ()
        }

        match query.TemporaryTables with
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
        (query : Query) = async {

        match query.TemporaryTables with
        | None -> ()
        | Some tables -> tables |> List.map (fun table -> table.SqlDrop)
                                |> (fun dropScripts -> String.Join("\n", dropScripts))
                                |> (fun dropScript  -> connection.ExecuteAsync(dropScript))
                                |> Async.AwaitTask
                                |> Async.RunSynchronously
                                |> ignore
    }

    let QueryAsync<'TRow> 
        (query : Query) 
        (connection : IDbConnection) = async {

        UseTemporaryTables connection query |> Async.RunSynchronously
        let! queryResult = await <| connection.QueryAsync<'TRow>(query.Script, parametersOf query)
        do!  ReleaseTemporaryTables connection query

        return queryResult
    }

    let QuerySingleAsync<'T>
        (query : Query)
        (connection : IDbConnection) = async {

        do!  UseTemporaryTables connection query
        let! single = await <| connection.QuerySingleOrDefaultAsync<'T>(query.Script, parametersOf query)
        do!  ReleaseTemporaryTables connection query

        return 
            if Object.ReferenceEquals(null, single) 
                then None
                else Some single 
    }

    let ExecuteAsync 
        (query : Query)
        (connection : IDbConnection) = async {

        do!  UseTemporaryTables connection query
        let! countOfAffectedRows = await <| connection.ExecuteAsync(query.Script, parametersOf query)
        do!  ReleaseTemporaryTables connection query

        return countOfAffectedRows
    }