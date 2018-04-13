namespace FSharp.Data.Dapper

open System.Data.SqlClient
open Dapper
open System.Collections


module SqlQueryAsyncBuilder =

    type SqlQueryState =
        { Script     : string option
          Tables     : Table list
          Values     : Table list
          Parameters : obj option }

    and Table = Table of 
                    Name : string * 
                    Rows : IEnumerable  

    type SqlQueryAsyncBuilder<'R> (connectionString: string) =

        member __.Run (state : SqlQueryState) = async {
            let script = 
                match state.Script with
                | Some x -> x
                | None   -> failwith "Script should not be empty"

            let connection = new SqlConnection(connectionString)

            let! r = connection.QuerySingleOrDefaultAsync<'R>(script) |> Async.AwaitTask
            
            return r
        }

        member __.Yield (()) = 
            { Script     = None
              Tables     = []
              Values     = []
              Parameters = None }

        [<CustomOperation("script")>]
        member __.Script (state, content : string) = 
            { state with Script = Some content } 

        [<CustomOperation("table")>]
        member __.Table (state, name, rows : IEnumerable) =            
            { state with Tables = Table (name, rows) :: state.Tables }
  
        [<CustomOperation("values")>]
        member __.Values (state, name, rows) = 
            { state with Values = Table (name, rows) :: state.Values }

        [<CustomOperation("parameters")>]
        member __.Parameters (state, parameters : obj) = 
            { state with Parameters = Some parameters } 

    let sqlQueryAsync<'R> connectionString = new SqlQueryAsyncBuilder<'R>(connectionString)
    
module testDbExecutor =
    let sqlQueryAsync<'R> = SqlQueryAsyncBuilder.sqlQueryAsync<'R>("some connection string")

    type User = 
        { Id       : int
          Login    : string 
          Password : string }

    (* examples of usage *)

    let find (id : int) = sqlQueryAsync<User option> {
        script "select * from users where id = @id"
        parameters [ "id", box id ]
    } 

    let findAll (identificators : int seq) = sqlQueryAsync<User seq> {
        script "select * from users u join #identificators i on u.Id = i.Value"
        values "identificators" identificators
    } 

    let save (user: User) = sqlQueryAsync<int> {
        script "if not exist by @id then insert else update, return id"
        parameters user
    }

    let saveAll (users : User seq) = sqlQueryAsync<User seq> {
        script "merge users from #users ..."
        table "users" users
    }