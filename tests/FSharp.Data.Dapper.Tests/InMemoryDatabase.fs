module InMemoryDatabase 

open Microsoft.Data.Sqlite

let private createConnectionString (dataSource : string ) = sprintf "Data Source = %s; Mode = Memory; Cache = Shared;" dataSource
let private createDedicatedConnectionString () = createConnectionString (sprintf "InMemory :: %s" (System.Guid.NewGuid().ToString()))

let private keepAliveConnectionString = createConnectionString "InMemory"
let private keepAliveConnection = new SqliteConnection(keepAliveConnectionString)

let Initialize () = keepAliveConnection.Open()
let Shutdown () = keepAliveConnection.Close()

let CreateSharedConnection () = new SqliteConnection(keepAliveConnectionString)
let CreateDedicatedConnection () = new SqliteConnection(createDedicatedConnectionString())
