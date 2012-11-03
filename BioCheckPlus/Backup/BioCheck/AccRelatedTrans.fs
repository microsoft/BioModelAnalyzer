(* Copyright (c) Microsoft Corporation. All rights reserved. *)

//#light

//#I "c:/Program Files (x86)/Microsoft Research/bin"
//#r @"Microsoft.Z3.dll"

module AccRelatedTrans

let AccClassification (accSet:string list) (transitions:string list) =

    
    let relTransSet =
        [ for acc in accSet do
            yield (List.filter (fun (x:string) -> x.Contains(acc)) transitions) ]
    //printf "%A" relTransSet
    let length = accSet.Length
    //let accIDSet = [0..length]
    //let accRelTransSet = List.zip accIDset relTransSet
    let mutable accID = 0
    let mutable accRelTransSet = Map.empty
    let mutable relTransSetMutable = relTransSet
    while not accSet.IsEmpty && not relTransSetMutable.IsEmpty do
        accRelTransSet <- accRelTransSet.Add(accID, relTransSetMutable.Head)
        accID <- accID + 1
        relTransSetMutable <- relTransSetMutable.Tail
    let TransSetForAcc = accRelTransSet
    // delete the Acc notations in all transitions
    let accDropedTransSetForAcc =
        [for i in [0..(length-1)] do
             yield TGBATransEncoding.ClassifyTrans (Map.find i TransSetForAcc)]
    accDropedTransSetForAcc