namespace FSharp.Data.Dapper

open QuerySingleAsyncBuilder
open QuerySingleOptionAsyncBuilder
open QuerySeqAsyncBuilder
   
module testDbExecutor =
    let querySingleAsync<'R> = QuerySingleAsyncBuilder<'R>(SqliteConnection null)
    let querySingleOptionAsync<'R> = QuerySingleOptionAsyncBuilder<'R>(SqliteConnection null)
    let querySeqAsync<'R> = QuerySeqAsyncBuilder<'R>(SqliteConnection null)

    type User = 
        { Id       : int
          Login    : string 
          Password : string }

    (* examples of usage *)

    let find (id : int) = querySingleOptionAsync<User> {
        script "select * from users where id = @id"
        parameters [ "id", box id ]
    } 

    let findAll (identificators : int seq) = querySeqAsync<User> {
        script "select * from users u join #identificators i on u.Id = i.Value"
        values "identificators" identificators
    } 

    let save (user: User) = querySingleAsync<int> {
        script "if not exist by @id then insert else update, return id"
        parameters user
    }

    let saveAll (users : User seq) = querySeqAsync<User> {
        script "merge users from #users ..."
        table "users" users
    }