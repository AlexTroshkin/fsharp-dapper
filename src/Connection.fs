namespace FSharp.Data.Dapper

open System.Data

type Connection =
    | SqlServerConnection of IDbConnection
    | SqliteConnection of IDbConnection

module Connection =
    let private unwrap = function
        | SqlServerConnection c -> c
        | SqliteConnection c -> c

    let Scope specificConnection f = using (unwrap specificConnection) (f specificConnection)