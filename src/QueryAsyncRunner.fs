namespace FSharp.Data.Dapper

open System
open System.Collections
open Dapper

module QueryAsyncRunner =

    type Table = 
        { Name : string 
          Rows : IEnumerable }

    type SqlQueryState =
        { Script     : string option
          Tables     : Table list
          Values     : Table list
          Parameters : obj option }

    let private unwrapConnection connection =
        match connection with
        | SqlServerConnection c -> c
        | SqliteConnection c -> c

    let private unwrapScript state =
        match state.Script with
        | Some s -> s
        | None   -> failwith "Script should not be empty"

    let private unwrapParameters state =
        match state.Parameters with
        | Some p -> p
        | None   -> null

    let runSingle<'R> state connection = async {
        let script = unwrapScript state
        let parameters = unwrapParameters state
        let db = unwrapConnection connection
        let query = db.QuerySingleAsync<'R>(script, parameters) |> Async.AwaitTask

        return! query
    }

    let runSingleOption<'R> state connection = async {
        use db = unwrapConnection connection        
        
        let script = unwrapScript state
        let parameters = unwrapParameters state
        let! result = db.QuerySingleOrDefaultAsync<'R>(script, parameters) |> Async.AwaitTask
        
        return
            if Object.ReferenceEquals (result, null) then
                None
            else 
                Some result            
    }

    let runSeq<'R> state connection = async {
        let script = unwrapScript state
        let parameters = unwrapParameters state
        let db = unwrapConnection connection
        let query = db.QueryAsync<'R>(script, parameters) |> Async.AwaitTask

        return! query
    }