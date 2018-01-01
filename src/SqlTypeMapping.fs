namespace FSharp.Data.Dapper

open System
open System.Data

module SqlTypeMapping =

        type TypeMapping =
            { CLR : Type
              SQL : DbType
              SQL_Name : string }

        let private TypeMap =  [
            { CLR = typeof<byte>           ; SQL = DbType.Byte              ; SQL_Name = "tinyint"}
            { CLR = typeof<sbyte>          ; SQL = DbType.SByte             ; SQL_Name = "smallint"}
            { CLR = typeof<int16>          ; SQL = DbType.Int16             ; SQL_Name = "smallint"}
            { CLR = typeof<uint16>         ; SQL = DbType.UInt16            ; SQL_Name = "int"}
            { CLR = typeof<int>            ; SQL = DbType.Int32             ; SQL_Name = "int"}
            { CLR = typeof<uint32>         ; SQL = DbType.UInt32            ; SQL_Name = "bigint"}
            { CLR = typeof<int64>          ; SQL = DbType.Int64             ; SQL_Name = "bigint"}
            { CLR = typeof<uint64>         ; SQL = DbType.UInt64            ; SQL_Name = "decimal(20}"}
            { CLR = typeof<float>          ; SQL = DbType.Single            ; SQL_Name = "real"}
            { CLR = typeof<double>         ; SQL = DbType.Double            ; SQL_Name = "float"}
            { CLR = typeof<decimal>        ; SQL = DbType.Decimal           ; SQL_Name = "decimal(29,4}"}
            { CLR = typeof<bool>           ; SQL = DbType.Boolean           ; SQL_Name = "bit"}
            { CLR = typeof<string>         ; SQL = DbType.String            ; SQL_Name = "nvarchar(max}"}
            { CLR = typeof<char>           ; SQL = DbType.StringFixedLength ; SQL_Name = "nchar"}
            { CLR = typeof<Guid>           ; SQL = DbType.Guid              ; SQL_Name = "UNIQUEIDENTIFIER"}
            { CLR = typeof<DateTime>       ; SQL = DbType.DateTime          ; SQL_Name = "datetime2"}
            { CLR = typeof<DateTimeOffset> ; SQL = DbType.DateTimeOffset    ; SQL_Name = "DATETIMEOFFSET"}
            { CLR = typeof<TimeSpan>       ; SQL = DbType.Time              ; SQL_Name = "time"}
            { CLR = typeof<byte array>     ; SQL = DbType.Binary            ; SQL_Name = "varbinary(max}"}
            { CLR = typeof<obj>            ; SQL = DbType.Object            ; SQL_Name = "sql_variant"}
        ]

        let tryFind clrType = 
            TypeMap |> List.tryFind (fun typeMapping -> typeMapping.CLR = clrType)