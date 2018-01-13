module Enums

open System

[<Flags>]
type DatabaseType =
    | SqlServer
    | Sqlite