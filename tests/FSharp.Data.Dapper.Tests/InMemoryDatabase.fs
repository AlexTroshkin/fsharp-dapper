module InMemoryDatabase 

open Microsoft.Data.Sqlite
open System.Data

    module Types =
        
        [<CLIMutable>]
        type Person = 
            { Id         : int64
              Name       : string
              Patronymic : string option
              Surname    : string }

    module Scripts =
        
        [<Literal>]
        let ``create person table`` = """
            create table Person (
                Id integer primary key,
                Name text not null,
                Patronymic text null,
                Surname text not null
            )
        """

        [<Literal>]
        let ``insert persons`` = """
            insert into Person (Name, Surname)
                values ('Иван', 'Иванов')
        """

    module Connection = 

        let private createConnectionString (dataSource : string ) = sprintf "Data Source = %s; Mode = Memory; Cache = Shared;" dataSource
        let private createDedicatedConnectionString () = createConnectionString (sprintf "InMemory :: %s" (System.Guid.NewGuid().ToString()))

        let private keepAliveConnectionString = createConnectionString "InMemory"
        let private keepAliveConnection = new SqliteConnection(keepAliveConnectionString)

        let Initialize () = keepAliveConnection.Open()
        let Shutdown () = keepAliveConnection.Close()

        let CreateSharedConnection () = new SqliteConnection(keepAliveConnectionString)
        let CreateDedicatedConnection 
            (scripts : string list option) = 
    
            let runScript script (con : IDbConnection) = 
                let command = con.CreateCommand()
                command.CommandText <- script
                command.ExecuteNonQuery() |> ignore

            let connection = new SqliteConnection(createDedicatedConnectionString())
            connection.Open()


            match scripts with
            | None -> ()
            | Some _scripts -> _scripts |> List.iter (fun script -> connection |> (runScript script))

            connection
