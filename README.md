# fsharp-dapper

The wrapper above the 'Dapper' library allows you to write more familiar code in the F # language. It also contains a functional for more simple work with temporary tables

## CI
[![Build status](https://ci.appveyor.com/api/projects/status/lx1gduy9wkx5edwy?svg=true)](https://ci.appveyor.com/project/AlexTroshkin/fsharp-dapper)
[![NuGet Badge](https://buildstats.info/nuget/FSharp.Data.Dapper)](https://www.nuget.org/packages/FSharp.Data.Dapper)

[![Build history](https://buildstats.info/appveyor/chart/AlexTroshkin/fsharp-dapper)](https://ci.appveyor.com/project/AlexTroshkin/fsharp-dapper/history)

## Support Option<'T> fields as parameters
```fsharp
type Person =
    { Id         : int
      Name       : string
      Patronymic : string option  // by default if you use this object in the query - you get an exception
      Surname    : string }
```
Add the following line when starting the application, so that the fields with the Option type <T> are treated like Nullable <T>
```fsharp
open FSharp.Data.Dapper

[<EntryPoint>]
let main argv =
    OptionHandler.RegisterTypes()
    
    // ...
```

## Sample Queries

###### QuerySingleAsync
```fsharp
open FSharp.Data.Dapper
open FSharp.Data.Dapper.Query.Parameters

let tryFindUser 
    (connection : IDbConnection) 
    (userId     : int          ) =

    let parameters = Parameters.Create [ "Id" <=> userId ]
    let script     = "select * from Users where Id = @Id"
    let query      = Query (script, parameters)
    
    let result = (query |> QuerySingleAsync<User> <| connection) |> Async.RunSynchronously
    
    // QuerySingleAsync return 'Some' when record is found and 'None' when not found
    match result with
    | Some user -> Some user
    | None      -> None
```    

###### QueryAsync
```fsharp
open FSharp.Data.Dapper

let getAllUsers (connection : IDbConnection) =

    let script = "select * from Users"
    let query  = Query (script)
    
    let users = (query |> QueryAsync<User> <| connection) |> Async.RunSynchronously
    
    users
```

###### ExecuteAsync
```fsharp
open FSharp.Data.Dapper

let updateUser
    (connection : IDbConnection)
    (user       : User) =

    let script = """
        update Users
            set Name     = @Name,
                Login    = @Login,
                Password = @Password
            where Id = @Id
    """
    
    let query  = Query(script, user) 
    let countOfAffectedRows = (query |> ExecuteAsync <| connection) |> Async.RunSynchronously
    
    ()
```

## Queries with temporary tables
The library provides 2 types of temporary tables, the first type with many columns and the second with one column (for example you need to send a list of identifiers to the query and nothing more)

At the moment, temporary tables are supported for the following databases:
- Microsoft SQL Server
- Sqlite

###### Temp table with one column
```fsharp
open FSharp.Data.Dapper
open FSharp.Data.Dapper.TempTable
open FSharp.Data.Dapper.Query.Parameters

let findPersons 
    (personIdentificators : int list     )
    (connection           : IDbConnection) =
    
    let tempTable    = TempTable.Create
                        ``Temp table with one column``("PersonIdentificators", "Id", personIdentificators)
                        DatabaseType.Sqlite

    let script = """
        select * from Person as p
            join PersonIdentificators as pi on
                p.Id = pi.Id
    """
    
    let query = Query (script, temporaryTables = [tempTable])
    
    (query |> QueryAsync <| connection) |> Async.RunSynchronously
```

###### Temp table with multiple columns
```fsharp
open FSharp.Data.Dapper
open FSharp.Data.Dapper.TempTable
open FSharp.Data.Dapper.Query.Parameters

let savePersons
    (persons    : person list  )
    (connection : IDbConnection) =

    let tempTable = TempTable.Create 
                        ``Temp table``("TPerson", persons)
                        DatabaseType.Sqlite

    // sqlite version 3.15
    let script = """
        update Person
            set (Name, Surname) = select (TPerson.Name, TPerson.Surname
                                            from TPerson
                                            where Person.Id = TPerson.Id)
                                            
        where exists (select 1 from TPerson where TPerson.Id = Person.Id)
    """

    let query = Query (script, temporaryTables = [tempTable])
    let countOfAffectedRows = (query |> ExecuteAsync <| connection) |> Async.RunSynchronously

    ()
```
