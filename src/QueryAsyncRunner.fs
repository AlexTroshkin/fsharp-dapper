namespace FSharp.Data.Dapper

open Dapper
open FSharp.Data.Dapper.Table
open System

module QueryAsyncRunner =

    type SqlQueryState =
        { Script     : string option
          Tables     : Table list
          Values     : Table list
          Parameters : obj option }

    let private unwrapScript state =
        match state.Script with
        | Some s -> s
        | None   -> failwith "Script should not be empty"

    let private unwrapParameters state =
        match state.Parameters with
        | Some p -> p
        | None   -> null

    let runSingle<'R> state specificConnection = async {
        let script = unwrapScript state
        let parameters = unwrapParameters state

        let row = Scope state.Tables state.Values specificConnection (fun connection -> 
            connection.QuerySingleAsync<'R>(script, parameters) |> Async.AwaitTask
        )

        return! row
    }

    let runSingleOption<'R> state specificConnection = async {        
        let script = unwrapScript state
        let parameters = unwrapParameters state

        let! row = Scope state.Tables state.Values specificConnection (fun connection -> 
            connection.QuerySingleOrDefaultAsync<'R>(script, parameters) |> Async.AwaitTask
        )
        
        return
            if Object.ReferenceEquals (row, null) then
                None
            else 
                Some row
    }

    let runSeq<'R> state specificConnection = async {
        let script = unwrapScript state
        let parameters = unwrapParameters state

        let rows = Scope state.Tables state.Values specificConnection (fun connection -> 
            connection.QueryAsync<'R>(script, parameters) |> Async.AwaitTask
        )

        return! rows
    }