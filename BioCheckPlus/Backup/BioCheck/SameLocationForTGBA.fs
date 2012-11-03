(* Copyright (c) Microsoft Corporation. All rights reserved. *)

//#light

//#I "c:/Program Files (x86)/Microsoft Research/bin"
//#r @"Microsoft.Z3.dll"

module SameLocationForTGBA

open Microsoft.Z3

let assertSameLocsForTGBA (locations:string list) (loopStart:int) (loopEnd:int) (z:Context)=
    let mutable assertionForOnePairOfLocs = z.MkTrue()
    let mutable locListToBeHandled = locations
    while not locListToBeHandled.IsEmpty do
        let mutable currLoc = locListToBeHandled.Head
    
        let mutable symOfStartStateLoc = sprintf "%s^%d" currLoc loopStart
        let mutable symOfEndStateLoc = sprintf "%s^%d" currLoc loopEnd
        let mutable nameOfStartStateLoc = z.MkConst(z.MkSymbol symOfStartStateLoc, z.MkIntSort())
        let mutable nameOfEndStateLoc = z.MkConst(z.MkSymbol symOfEndStateLoc, z.MkIntSort())
        let mutable assertionForSameLoc = z.MkEq(nameOfStartStateLoc, nameOfEndStateLoc)
        assertionForOnePairOfLocs <- z.MkAnd(assertionForOnePairOfLocs, assertionForSameLoc)
        locListToBeHandled <- locListToBeHandled.Tail
    assertionForOnePairOfLocs

