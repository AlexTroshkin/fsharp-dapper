namespace FSharp.Data.Dapper

open System.Data

type Connection =
    | SqlServerConnection of IDbConnection
    | SqliteConnection of IDbConnection