namespace FSharp.Data.Dapper

open System
open System.Data

module SqlTypeMapping =

        let private TypeMap =  [
            (typeof<byte>,  DbType.Byte)
            (typeof<sbyte>,  DbType.SByte)
            (typeof<int16>,  DbType.Int16)
            (typeof<uint16>,  DbType.UInt16)
            (typeof<int>,  DbType.Int32)
            (typeof<uint32>,  DbType.UInt32)
            (typeof<int64>,  DbType.Int64)
            (typeof<uint64>,  DbType.UInt64)
            (typeof<float>,  DbType.Single)
            (typeof<double>,  DbType.Double)
            (typeof<decimal>,  DbType.Decimal)
            (typeof<bool>,  DbType.Boolean)
            (typeof<string>,  DbType.String)
            (typeof<char>,  DbType.StringFixedLength)
            (typeof<Guid>,  DbType.Guid)
            (typeof<DateTime>,  DbType.DateTime)
            (typeof<DateTimeOffset>,  DbType.DateTimeOffset)
            (typeof<TimeSpan>,  DbType.Time)
            (typeof<byte>,  DbType.Binary)
            (typeof<byte Nullable>,  DbType.Byte)
            (typeof<sbyte Nullable>,  DbType.SByte)
            (typeof<int16 Nullable >,  DbType.Int16)
            (typeof<uint16 Nullable>,  DbType.UInt16)
            (typeof<int Nullable>,  DbType.Int32)
            (typeof<uint32 Nullable>,  DbType.UInt32)
            (typeof<int64 Nullable>,  DbType.Int64)
            (typeof<uint64 Nullable>,  DbType.UInt64)
            (typeof<float Nullable>,  DbType.Single)
            (typeof<double Nullable>,  DbType.Double)
            (typeof<decimal Nullable>,  DbType.Decimal)
            (typeof<bool Nullable>,  DbType.Boolean)
            (typeof<char Nullable>,  DbType.StringFixedLength)
            (typeof<Guid Nullable>,  DbType.Guid)
            (typeof<DateTime Nullable>,  DbType.DateTime)
            (typeof<DateTimeOffset Nullable>,  DbType.DateTimeOffset)
            (typeof<TimeSpan Nullable>,  DbType.Time)
            (typeof<obj>,  DbType.Object)
        ]

        let toSqlType sourceType = 
            TypeMap |> List.tryFind (fun (key, _) -> key = sourceType)
                    |> Option.map (fun (_, sqlType) -> sqlType)