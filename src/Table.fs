namespace FSharp.Data.Dapper

open System
open System.Reflection
open System.Data
open System.Data.Common

module Table =

    module TypeMapping =

        type CollumnType = 
            { SqlName   : string 
              Parameter : string option
              ClrType   : Type }

        let private sqliteMappings = [
            { SqlName = "Bit"              ; Parameter = None          ; ClrType = typedefof<bool>           }
            { SqlName = "TinyInt"          ; Parameter = None          ; ClrType = typedefof<byte>           }
            { SqlName = "SamllInt"         ; Parameter = None          ; ClrType = typedefof<sbyte>          }
            { SqlName = "SamllInt"         ; Parameter = None          ; ClrType = typedefof<int16>          }
            { SqlName = "Int"              ; Parameter = None          ; ClrType = typedefof<uint16>         }
            { SqlName = "Int"              ; Parameter = None          ; ClrType = typedefof<int32>          }        
            { SqlName = "BigInt"           ; Parameter = None          ; ClrType = typedefof<uint32>         }
            { SqlName = "BigInt"           ; Parameter = None          ; ClrType = typedefof<int64>          }        
            { SqlName = "Decimal"          ; Parameter = Some "(20)"   ; ClrType = typedefof<uint64>         }
                
            { SqlName = "Real"             ; Parameter = None          ; ClrType = typedefof<single>         }
            { SqlName = "Float"            ; Parameter = None          ; ClrType = typedefof<float>          }
            { SqlName = "Float"            ; Parameter = None          ; ClrType = typedefof<double>         }
            { SqlName = "Deciaml"          ; Parameter = Some "(29,4)" ; ClrType = typedefof<decimal>        }
         
            { SqlName = "NChar"            ; Parameter = Some "(1)"    ; ClrType = typedefof<char>           }
            { SqlName = "NText"            ; Parameter = None          ; ClrType = typedefof<string>         }
          
            { SqlName = "UniqueIdentifier" ; Parameter = None          ; ClrType = typedefof<Guid>           }
          
            { SqlName = "DateTime2"        ; Parameter = None          ; ClrType = typedefof<DateTime>       }
            { SqlName = "DateTimeOffset"   ; Parameter = None          ; ClrType = typedefof<DateTimeOffset> }
            { SqlName = "Time"             ; Parameter = None          ; ClrType = typedefof<TimeSpan>       }
 
            { SqlName = "VarBinary"        ; Parameter = Some "(max)"  ; ClrType = typedefof<byte array>     }
        ]

        let private find mappings clrType = mappings |> List.find (fun m -> m.ClrType = clrType)

        let Find connection clrType = 
            match connection with
            | SqliteConnection _ -> find sqliteMappings clrType
            | SqlServerConnection _ -> failwith "not supported at this moment"

    module ReflectionExtension =

        let GetTypeOfSeq seq =
            match seq |> Seq.tryHead with
            | Some head -> head.GetType()
            | None -> 
                let seqType = seq.GetType()
                match seqType.IsGenericTypeDefinition with
                | false -> failwith "The collection type could not be determined, it is not generalized and has no elements"
                | true  -> seqType.GenericTypeArguments |> Array.head
        
        let (|Nullable|_|) (clrType : Type) =
            if clrType.IsGenericType then
                let genericTypeDef = clrType.GetGenericTypeDefinition()
                if  genericTypeDef = typedefof<option<_>> || 
                    genericTypeDef = typedefof<Nullable<_>> then
                   
                   Some (Array.head clrType.GenericTypeArguments)
                else
                    None
            else 
                None

        let (|Primitive|_|) (clrType : Type) =
            if clrType.IsPrimitive || 
               clrType = typeof<Enum> ||
               clrType = typeof<decimal> ||
               clrType = typeof<string> ||
               clrType = typeof<DateTime> ||
               clrType = typeof<TimeSpan> ||
               clrType = typeof<DateTimeOffset> ||
               clrType = typeof<Guid> then

               (* TODO: Add rec check on nullable and option *)

                Some ()
            else
                None

    module Schema =

        type Column = 
            { Name      : string
              Type      : TypeMapping.CollumnType
              AllowNull : bool }

        type Table = 
            { Name    : string 
              Columns : Column array }    

        let private mkColumn connection name clrType =
            match clrType with
            | ReflectionExtension.Nullable contentType ->
                { Name = name 
                  Type = TypeMapping.Find connection contentType
                  AllowNull = true }
            | _ -> 
                { Name = name 
                  Type = TypeMapping.Find connection clrType
                  AllowNull = false }

        (*
            Make columns from type of sequence (rows)
            If the sequence type is a primitive (e.g. this could be a seq of IDs), then only one column named 'Value' will be created.
            In other cases, columns will be created to match the properties of the sequence type
        *)
        
        let private mkColumns connection rows =
            let rowType = ReflectionExtension.GetTypeOfSeq rows

            match rowType with
            | ReflectionExtension.Primitive -> 
                [| mkColumn connection "Value" rowType |]
            | _ -> 
                rowType.GetProperties(
                    BindingFlags.Instance ||| 
                    BindingFlags.Public   ||| 
                    BindingFlags.GetProperty) 
                |> Array.map (fun property ->
                    mkColumn connection property.Name property.PropertyType)

        let private mkName connection nameOfTable =
            match connection with
            | SqliteConnection _ -> nameOfTable
            | SqlServerConnection _ -> failwith "not supported at this moment"
        
        let Create connection nameOfTable rows =
            { Name    = mkName connection nameOfTable
              Columns = mkColumns connection rows }

    module Scripts =

        type Scripts = 
            { Create : string 
              Drop   : string }       

        let private mkSqliteScripts (scheme : Schema.Table) = 
            let columnToSql (column : Schema.Column) = 
                sprintf "%s %s %s"
                    column.Name                    
                    (match column.Type.Parameter with
                     | Some parameter -> sprintf "%s%s" column.Type.SqlName parameter
                     | None           -> column.Type.SqlName)
                    (if column.AllowNull then "NULL" else "NOT NULL")

            let createScript =
                sprintf "CREATE TEMP TABLE %s (%s)"
                    scheme.Name
                    (scheme.Columns 
                        |> Array.map columnToSql
                        |> String.concat ",")

            let dropSCript =
                sprintf "DROP TABLE %s" scheme.Name

            { Create = createScript
              Drop   = dropSCript }

        let Create connection (scheme : Schema.Table) = 
            match connection with
            | SqliteConnection _ -> mkSqliteScripts scheme
            | SqlServerConnection _ -> failwith "not supported at this moment"

    module Data =
        open Schema

        let private (|Values|Table|) scheme =
            if scheme.Columns.Length = 1 then
                Values scheme.Columns.[0]
            else
                Table scheme.Columns

        let Create scheme (rows : seq<'a>) =
            let table = new DataTable(scheme.Name)

            match scheme with
            | Values column -> 
                table.Columns.Add (new DataColumn(column.Name, column.Type.ClrType))
                for row in rows do table.Rows.Add ([|box row|]) |> ignore
                table                
            | Table columns -> 
                columns |> Array.map (fun column -> new DataColumn (column.Name, column.Type.ClrType))
                        |> table.Columns.AddRange
                
                for row in rows do 
                    columns |> Array.map (fun column -> 
                                let value = row.GetType().GetProperty(column.Name).GetValue(row)
                                if Object.ReferenceEquals(null, value) then 
                                    (DBNull.Value :> obj)
                                else
                                    value) 
                            |> table.Rows.Add
                            |> ignore                    

                table
    
    module Operations =

        module private BulkInsert =
            let mkInsertScript dataTableName parameterNames =
                sprintf "INSERT INTO %s VALUES (%s)" dataTableName (String.concat "," parameterNames)

            let mkParameterNames columns = 
                columns |> Seq.cast<DataColumn>
                        |> Seq.map (fun column -> sprintf "@%s" column.ColumnName)

            let mkParameter (command : IDbCommand) name =
                let parameter = command.CreateParameter()
                parameter.ParameterName <- name
                parameter

            let mkCommand (connection : IDbConnection) script =
                let command = connection.CreateCommand()
                command.CommandText <- script
                command

        let BulkInsert connection (dataTable : DataTable) = 
            let parameterNames = BulkInsert.mkParameterNames dataTable.Columns
            let insertScript   = BulkInsert.mkInsertScript dataTable.TableName parameterNames
            let command        = BulkInsert.mkCommand connection insertScript

            parameterNames 
            |> Seq.map ((BulkInsert.mkParameter command) >> command.Parameters.Add)
            |> ignore
            
            let parameters = command.Parameters |> Seq.cast<DbParameter>

            for row in dataTable.Rows do                
                parameters 
                |> Seq.iter2 (fun parameter value -> parameter.Value <- value) 
                <| row.ItemArray

                command.ExecuteNonQuery() |> ignore