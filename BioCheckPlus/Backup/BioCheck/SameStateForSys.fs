(* Copyright (c) Microsoft Corporation. All rights reserved. *)

//#light

//#I "c:/Program Files (x86)/Microsoft Research/bin"
//#r @"Microsoft.Z3.dll"

module SameStateForSys

open Microsoft.Z3

let assertSameStatesForSys (network:QN.node list) (loopStart:int) (loopEnd:int) (z:Context)=
    let mutable assertionForOnePairOfStates = z.MkTrue()
    let mutable nodeListToBeHandled = network
    while not nodeListToBeHandled.IsEmpty do
        let mutable currNode = nodeListToBeHandled.Head
    
        let mutable symOfStartStateVar = BioCheckZ3.get_z3_int_var_at_time currNode loopStart
        let mutable symOfEndStateVar = BioCheckZ3.get_z3_int_var_at_time currNode loopEnd
        let mutable nameOfStartStateVar = z.MkConst(z.MkSymbol symOfStartStateVar, z.MkIntSort())
        let mutable nameOfEndStateVar = z.MkConst(z.MkSymbol symOfEndStateVar, z.MkIntSort())
        let mutable assertionForSameVar = z.MkEq(nameOfStartStateVar, nameOfEndStateVar)
        assertionForOnePairOfStates <- z.MkAnd(assertionForOnePairOfStates, assertionForSameVar)
        nodeListToBeHandled <- nodeListToBeHandled.Tail
    assertionForOnePairOfStates