namespace FSharp.Data.Dapper

open System.Data

[<StructuralComparison>]
[<StructuralEquality>]
type SqlTypeRepresentation (asEnum : SqlDbType, asString : string, ?parameters : string list) = 
    struct
        member __.AsEnum = asEnum
        member __.AsString = asString
        member __.Parameters = parameters

        member __.AsStringWithParaeters  =
            match __.Parameters with
            | None    -> asString
            | Some xs ->
                let parametersAsString = String.concat "," xs
                sprintf "%s(%s)" __.AsString parametersAsString
    end