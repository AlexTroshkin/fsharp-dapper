namespace FSharp.Data.Dapper

open System
open System.Data
open Dapper

    type QueryDefinition = 
        { Script     : string
          Parameters : obj option }

[<AutoOpen>]
module Database =
    
    let private await task = task |> Async.AwaitTask

    let private parametersOf qDef =
        match qDef.Parameters with
        | Some p -> p
        | None -> null


    let queryAsync<'TRow> 
        (queryDefinition : QueryDefinition) 
        (connection : IDbConnection) = async {

        return! await <| connection.QueryAsync<'TRow>(queryDefinition.Script, parametersOf queryDefinition)
    }

    let querySingleAsync<'T>
        (queryDefinition : QueryDefinition)
        (connection : IDbConnection) = async {

        let! single = await <| connection.QuerySingleOrDefaultAsync<'T>(queryDefinition.Script, parametersOf queryDefinition)

        return 
            if Object.ReferenceEquals(null, single) 
                then None
                else Some single 
    }

    let executeAsync 
        (queryDefinition : QueryDefinition)
        (connection : IDbConnection) = async {

        let! countOfAffectedRows = await <| connection.ExecuteAsync(queryDefinition.Script, parametersOf queryDefinition)

        return countOfAffectedRows
    }