namespace FSharp.Data.Dapper

open Dapper
open System

type OptionHandler<'T> () =
    inherit SqlMapper.TypeHandler<option<'T>> ()

    override __.SetValue (param, value) =
        let valueOrNull =
            match value with
            | Some x -> box x
            | None   -> null

        param.Value <- valueOrNull

    override __.Parse value =
        if 
            Object.ReferenceEquals(value, null) || 
            value = box DBNull.Value
        then None
        else Some (value :?> 'T)

module OptionHandler =
    
    let RegisterTypes () =
        SqlMapper.AddTypeHandler (OptionHandler<bool>())
        SqlMapper.AddTypeHandler (OptionHandler<byte>())
        SqlMapper.AddTypeHandler (OptionHandler<sbyte>())
        SqlMapper.AddTypeHandler (OptionHandler<int16>())
        SqlMapper.AddTypeHandler (OptionHandler<uint16>())
        SqlMapper.AddTypeHandler (OptionHandler<int32>())
        SqlMapper.AddTypeHandler (OptionHandler<uint32>())
        SqlMapper.AddTypeHandler (OptionHandler<int64>())
        SqlMapper.AddTypeHandler (OptionHandler<uint64>())
        SqlMapper.AddTypeHandler (OptionHandler<single>())
        SqlMapper.AddTypeHandler (OptionHandler<float>())
        SqlMapper.AddTypeHandler (OptionHandler<double>())
        SqlMapper.AddTypeHandler (OptionHandler<decimal>())
        SqlMapper.AddTypeHandler (OptionHandler<char>())
        SqlMapper.AddTypeHandler (OptionHandler<string>())
        SqlMapper.AddTypeHandler (OptionHandler<Guid>())
        SqlMapper.AddTypeHandler (OptionHandler<DateTime>())
        SqlMapper.AddTypeHandler (OptionHandler<DateTimeOffset>())
        SqlMapper.AddTypeHandler (OptionHandler<TimeSpan>())
        SqlMapper.AddTypeHandler (OptionHandler<DateTimeOffset>())
        SqlMapper.AddTypeHandler (OptionHandler<obj>())