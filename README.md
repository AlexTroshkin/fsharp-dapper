# fsharp-dapper

The wrapper above the 'Dapper' library allows you to write more familiar code in the F # language. It also contains a functional for more simple work with temporary tables

## CI
[![Build status](https://ci.appveyor.com/api/projects/status/lx1gduy9wkx5edwy?svg=true)](https://ci.appveyor.com/project/AlexTroshkin/fsharp-dapper)
[![NuGet Badge](https://buildstats.info/nuget/FSharp.Data.Dapper)](https://www.nuget.org/packages/FSharp.Data.Dapper)

[![Build history](https://buildstats.info/appveyor/chart/AlexTroshkin/fsharp-dapper)](https://ci.appveyor.com/project/AlexTroshkin/fsharp-dapper/history)

## Sample Queries

###### QuerySingleAsync
```fsharp
open FSharp.Data.Dapper
open FSharp.Data.Dapper.Query.Parameters

let tryFindUser 
    (connection : IDbConnection) 
    (userId     : int          )

    let parameters = Parameters.Create [ "Id" <=> userId ]
    let script     = "select * from Users where Id = @Id"
    let query      = Query (script, parameters)
    
    let result = (query |> QuerySingleAsync <| connection) |> Async.RunSynchronously
    
    // QuerySingleAsync return 'Some' when record is found and 'None' when not found
    match result with
    | Some user -> Some user
    | None      -> None
```    
