namespace FSharp.Data.Dapper

open System
open System.Data

type SqlClrTypeCorellation =
    { SqlTypeRepresentation : SqlTypeRepresentation
      ClrTypeRepresentation : Type }

module SqlClrTypeCorellation =
        open Enums

        module SqlServer =
            let corellations = [
                { SqlTypeRepresentation = SqlTypeRepresentation(SqlDbType.Bit, "Bit")
                  ClrTypeRepresentation = typedefof<bool> }
                { SqlTypeRepresentation = SqlTypeRepresentation(SqlDbType.TinyInt, "TinyInt")
                  ClrTypeRepresentation = typedefof<byte> }
                { SqlTypeRepresentation = SqlTypeRepresentation(SqlDbType.SmallInt, "SamllInt")
                  ClrTypeRepresentation = typedefof<sbyte> }
                { SqlTypeRepresentation = SqlTypeRepresentation(SqlDbType.SmallInt, "SamllInt")
                  ClrTypeRepresentation = typedefof<int16> }
                { SqlTypeRepresentation = SqlTypeRepresentation(SqlDbType.Int, "Int")
                  ClrTypeRepresentation = typedefof<uint16> }
                { SqlTypeRepresentation = SqlTypeRepresentation(SqlDbType.Int, "Int")
                  ClrTypeRepresentation = typedefof<int32> }
                { SqlTypeRepresentation = SqlTypeRepresentation(SqlDbType.BigInt, "BigInt")
                  ClrTypeRepresentation = typedefof<uint32> }
                { SqlTypeRepresentation = SqlTypeRepresentation(SqlDbType.BigInt, "BigInt")
                  ClrTypeRepresentation = typedefof<int64> }
                { SqlTypeRepresentation = SqlTypeRepresentation(SqlDbType.Decimal, "Decimal", ["20"])
                  ClrTypeRepresentation = typedefof<uint64> }

                { SqlTypeRepresentation = SqlTypeRepresentation(SqlDbType.Real, "Real")
                  ClrTypeRepresentation = typedefof<single> }
                { SqlTypeRepresentation = SqlTypeRepresentation(SqlDbType.Float, "Float")
                  ClrTypeRepresentation = typedefof<float> }
                { SqlTypeRepresentation = SqlTypeRepresentation(SqlDbType.Float, "Float")
                  ClrTypeRepresentation = typedefof<double> }
                { SqlTypeRepresentation = SqlTypeRepresentation(SqlDbType.Decimal, "Deciaml", ["29"; "4"])
                  ClrTypeRepresentation = typedefof<decimal> }

                { SqlTypeRepresentation = SqlTypeRepresentation(SqlDbType.NChar, "NChar", ["1"])
                  ClrTypeRepresentation = typedefof<char> }
                { SqlTypeRepresentation = SqlTypeRepresentation(SqlDbType.Bit, "NVarChar", ["Max"])
                  ClrTypeRepresentation = typedefof<string> }
                { SqlTypeRepresentation = SqlTypeRepresentation(SqlDbType.UniqueIdentifier, "UniqueIdentifier")
                  ClrTypeRepresentation = typedefof<Guid> }

                { SqlTypeRepresentation = SqlTypeRepresentation(SqlDbType.DateTime2, "DateTime2")
                  ClrTypeRepresentation = typedefof<DateTime> }
                { SqlTypeRepresentation = SqlTypeRepresentation(SqlDbType.DateTimeOffset, "DateTimeOffset")
                  ClrTypeRepresentation = typedefof<DateTimeOffset> }
                { SqlTypeRepresentation = SqlTypeRepresentation(SqlDbType.Time, "Time")
                  ClrTypeRepresentation = typedefof<TimeSpan> }

                { SqlTypeRepresentation = SqlTypeRepresentation(SqlDbType.VarBinary, "VarBinary", ["Max"])
                  ClrTypeRepresentation = typedefof<DateTimeOffset> }
                { SqlTypeRepresentation = SqlTypeRepresentation(SqlDbType.Variant, "Sql_Variant")
                  ClrTypeRepresentation = typedefof<obj> }
            ]

        module Sqlite =
            let corellations = [
                { SqlTypeRepresentation = SqlTypeRepresentation(SqlDbType.Bit, "Bit")
                  ClrTypeRepresentation = typedefof<bool> }
                { SqlTypeRepresentation = SqlTypeRepresentation(SqlDbType.TinyInt, "TinyInt")
                  ClrTypeRepresentation = typedefof<byte> }
                { SqlTypeRepresentation = SqlTypeRepresentation(SqlDbType.SmallInt, "SamllInt")
                  ClrTypeRepresentation = typedefof<sbyte> }
                { SqlTypeRepresentation = SqlTypeRepresentation(SqlDbType.SmallInt, "SamllInt")
                  ClrTypeRepresentation = typedefof<int16> }
                { SqlTypeRepresentation = SqlTypeRepresentation(SqlDbType.Int, "Int")
                  ClrTypeRepresentation = typedefof<uint16> }
                { SqlTypeRepresentation = SqlTypeRepresentation(SqlDbType.Int, "Int")
                  ClrTypeRepresentation = typedefof<int32> }
                { SqlTypeRepresentation = SqlTypeRepresentation(SqlDbType.BigInt, "BigInt")
                  ClrTypeRepresentation = typedefof<uint32> }
                { SqlTypeRepresentation = SqlTypeRepresentation(SqlDbType.BigInt, "BigInt")
                  ClrTypeRepresentation = typedefof<int64> }
                { SqlTypeRepresentation = SqlTypeRepresentation(SqlDbType.Decimal, "Decimal", ["20"])
                  ClrTypeRepresentation = typedefof<uint64> }

                { SqlTypeRepresentation = SqlTypeRepresentation(SqlDbType.Real, "Real")
                  ClrTypeRepresentation = typedefof<single> }
                { SqlTypeRepresentation = SqlTypeRepresentation(SqlDbType.Float, "Float")
                  ClrTypeRepresentation = typedefof<float> }
                { SqlTypeRepresentation = SqlTypeRepresentation(SqlDbType.Float, "Float")
                  ClrTypeRepresentation = typedefof<double> }
                { SqlTypeRepresentation = SqlTypeRepresentation(SqlDbType.Decimal, "Deciaml", ["29"; "4"])
                  ClrTypeRepresentation = typedefof<decimal> }

                { SqlTypeRepresentation = SqlTypeRepresentation(SqlDbType.NChar, "NChar", ["1"])
                  ClrTypeRepresentation = typedefof<char> }
                { SqlTypeRepresentation = SqlTypeRepresentation(SqlDbType.Bit, "NVarChar", ["Max"])
                  ClrTypeRepresentation = typedefof<string> }
                { SqlTypeRepresentation = SqlTypeRepresentation(SqlDbType.UniqueIdentifier, "UniqueIdentifier")
                  ClrTypeRepresentation = typedefof<Guid> }

                { SqlTypeRepresentation = SqlTypeRepresentation(SqlDbType.DateTime2, "DateTime2")
                  ClrTypeRepresentation = typedefof<DateTime> }
                { SqlTypeRepresentation = SqlTypeRepresentation(SqlDbType.DateTimeOffset, "DateTimeOffset")
                  ClrTypeRepresentation = typedefof<DateTimeOffset> }
                { SqlTypeRepresentation = SqlTypeRepresentation(SqlDbType.Time, "Time")
                  ClrTypeRepresentation = typedefof<TimeSpan> }

                { SqlTypeRepresentation = SqlTypeRepresentation(SqlDbType.VarBinary, "VarBinary", ["Max"])
                  ClrTypeRepresentation = typedefof<DateTimeOffset> }
                { SqlTypeRepresentation = SqlTypeRepresentation(SqlDbType.Variant, "Sql_Variant")
                  ClrTypeRepresentation = typedefof<obj> }
            ]

        let private findCorellations (dbType : DatabaseType) = 
            match dbType with
            | SqlServer -> SqlServer.corellations
            | Sqlite    -> Sqlite.corellations
            | _         -> failwith (sprintf "Unsupported database %A for find corellations" dbType)

        let TryFindCorellation dbType clrTypeRepresentation =
            dbType
            |> findCorellations
            |> List.tryFind (fun corellation -> corellation.ClrTypeRepresentation = clrTypeRepresentation)