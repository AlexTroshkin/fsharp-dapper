namespace FSharp.Data.Dapper

open System.Collections

[<AutoOpen>]
module TemporaryTable =

    type TemporaryTableDefinition = 
        { Name : string 
          Rows : IEnumerable }
