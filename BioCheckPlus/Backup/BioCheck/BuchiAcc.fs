(* Copyright (c) Microsoft Corporation. All rights reserved. *)

//#light

//#I "c:/Program Files (x86)/Microsoft Research/bin"
//#r @"Microsoft.Z3.dll"


module BuchiAcc

open Microsoft.Z3

let Z3ForAccCon (accRelTransSet:string list list list) (loopStart:int) (loopEnd:int) (z:Context)=
    let mutable assertionForAllAccSubsets = z.MkTrue()
    let mutable accSubsets = accRelTransSet
    while not accSubsets.IsEmpty do
        let currAccSubset = accSubsets.Head   
        // each member is a set of transitions representing a sub-Buchi condition f
        let mutable assertionForOneAccSubset = z.MkFalse()
        let mutable i = loopStart
        while i <= loopEnd do
           let allTransInCurrSubset = TGBATransEncoding.assertTransAtOneStep currAccSubset i z
           assertionForOneAccSubset <- z.MkOr(assertionForOneAccSubset, allTransInCurrSubset)
           i <- i + 1
        assertionForAllAccSubsets <- z.MkAnd(assertionForAllAccSubsets, assertionForOneAccSubset)
        accSubsets <- accSubsets.Tail
    assertionForAllAccSubsets

