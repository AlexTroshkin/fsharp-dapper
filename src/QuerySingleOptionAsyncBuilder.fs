namespace FSharp.Data.Dapper 

open QueryAsyncRunner
open System.Collections

[<AutoOpen>]
module QuerySingleOptionAsyncBuilder =

    type QuerySingleOptionAsyncBuilder<'R> (connectionF: unit -> Connection) =

        member __.Run state = runSingleOption<'R> state (connectionF())

        member __.Yield (()) = 
            { Script     = None
              Tables     = []
              Values     = []
              Parameters = None }

        [<CustomOperation("script")>]
        member __.Script (state, content : string) = 
            { state with Script = Some content } 

        [<CustomOperation("table")>]
        member __.Table (state, name : string, rows : IEnumerable) =            
            { state with Tables = { Name = name; Rows = rows } :: state.Tables }
  
        [<CustomOperation("values")>]
        member __.Values (state, name, rows) = 
            { state with Values = { Name = name; Rows = rows } :: state.Values }

        [<CustomOperation("parameters")>]
        member __.Parameters (state, parameters : obj) = 
            { state with Parameters = Some parameters } 

    let querySingleOptionAsync<'R> connectionF = new QuerySingleOptionAsyncBuilder<'R>(connectionF)