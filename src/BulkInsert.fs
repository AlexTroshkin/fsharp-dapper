namespace FSharp.Data.Dapper

module BulkInsert =
    open System.Data
    open System.Data.Common
    
    let private ``create insert script`` nameOfTable parameterNames =
        let values = String.concat "," parameterNames
        sprintf "INSERT INTO %s values (%s)" nameOfTable values

    let private ``create parameter names`` (dataOfTable : DataTable) =
        dataOfTable.Columns 
        |> Seq.cast<DataColumn>
        |> Seq.map (fun col -> sprintf "@%s" col.ColumnName)

    let private ``create command`` 
        (connection  : IDbConnection)
        (dataOfTable : DataTable) =

        let parameterNames = ``create parameter names`` dataOfTable
        let insertScript   = ``create insert script`` dataOfTable.TableName parameterNames

        let command = connection.CreateCommand()
        command.CommandText <- insertScript
        
        for parameterName in parameterNames do
            let parameter = command.CreateParameter()
            parameter.ParameterName <- parameterName
            command.Parameters.Add(parameter) |> ignore
        
        command

    let Execute 
        (connection  : IDbConnection)
        (dataOfTable : DataTable) =
        
        let command = ``create command`` connection dataOfTable

        for row in dataOfTable.Rows do  
            command.Parameters
            |> Seq.cast<DbParameter>
            |> Seq.iteri (fun idx parameter -> parameter.Value <- row.[dataOfTable.Columns.[idx].ColumnName])
            
            command.ExecuteNonQuery() |> ignore
        
        ()