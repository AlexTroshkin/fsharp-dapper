namespace FSharp.Data.Dapper

open System
open System.Collections
open System.Reflection

module Reflection =
    
    let TryGetTypeOfSeq (seqence : IEnumerable) =
        match seqence |> Seq.cast<obj> |> Seq.tryHead with
        | Some head -> Some (head.GetType())
        | None -> 
            let tableType = seqence.GetType()
            match tableType.IsGenericTypeDefinition with
            | false -> None 
            | true  -> tableType.GenericTypeArguments |> Array.tryHead
    
    let TryGetUnderlyingType (clrType : Type) = 
        let genericTypeDef = 
            match clrType.IsGenericType with 
            | true -> Some (clrType.GetGenericTypeDefinition())
            | false -> None 

        match genericTypeDef with
        | None -> None
        | Some typeDef -> 
            let hasUnderlyingType = (typeDef = typedefof<option<_>> || typeDef = typedefof<Nullable<_>>)
            match hasUnderlyingType with
            | true  -> clrType.GenericTypeArguments |> Array.tryHead
            | false -> None


