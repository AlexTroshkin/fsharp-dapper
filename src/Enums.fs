namespace FSharp.Data.Dapper

open System

module Enums =

    [<Flags>]
    type DatabaseType =
        | SqlServer
        | Sqlite