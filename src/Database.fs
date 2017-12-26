namespace FSharp.Data.Dapper

open Dapper
open System.Data
open Microsoft.FSharp.Core.Operators.Unchecked

    type QueryDefinition = 
        { Script : string
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

        return await <| connection.QueryAsync<'TRow>(queryDefinition.Script, parametersOf queryDefinition)
    }

    let querySingleOrDefault<'T when 'T : equality >
        (queryDefinition : QueryDefinition)
        (connection : IDbConnection) = async {

        let! single = await <| connection.QuerySingleOrDefaultAsync<'T>(queryDefinition.Script, parametersOf queryDefinition)

        return 
            match single with
            | value when value = defaultof<'T> -> None
            | value -> Some value
    }

    let executeAsync 
        (queryDefinition : QueryDefinition)
        (connection : IDbConnection) = async {

        let countOfAffectedRows = await <| connection.ExecuteAsync(queryDefinition.Script, parametersOf queryDefinition)

        return countOfAffectedRows
    }