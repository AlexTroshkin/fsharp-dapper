namespace FSharp.Data.Dapper

open System
open System.Data
open Dapper

[<AutoOpen>]
module Query =
    
    type QueryDefinition = 
        { Script          : string
          Parameters      : obj option
          TemporaryTables : TableDefinition list option }
    
    let DefaultQueryDefinition = 
        { Script          = String.Empty
          Parameters      = None
          TemporaryTables = None }

    let private await task = task |> Async.AwaitTask
    let private parametersOf queryDefinition =
        match queryDefinition.Parameters with
        | Some p -> p
        | None -> null


    let QueryAsync<'TRow> 
        (queryDefinition : QueryDefinition) 
        (connection : IDbConnection) = async {

        return! await <| connection.QueryAsync<'TRow>(queryDefinition.Script, parametersOf queryDefinition)
    }

    let QuerySingleAsync<'T>
        (queryDefinition : QueryDefinition)
        (connection : IDbConnection) = async {

        let! single = await <| connection.QuerySingleOrDefaultAsync<'T>(queryDefinition.Script, parametersOf queryDefinition)

        return 
            if Object.ReferenceEquals(null, single) 
                then None
                else Some single 
    }

    let ExecuteAsync 
        (queryDefinition : QueryDefinition)
        (connection : IDbConnection) = async {

        let! countOfAffectedRows = await <| connection.ExecuteAsync(queryDefinition.Script, parametersOf queryDefinition)

        return countOfAffectedRows
    }