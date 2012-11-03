(* Copyright (c) Microsoft Corporation. All rights reserved. *)

//#light

//#I "c:/Program Files (x86)/Microsoft Research/bin"
//#r @"Microsoft.Z3.dll"

module testing1

open Microsoft.Z3

let test1 (z : Context) =
    let testnamestr = "v30^11"
    let testname = z.MkConst(z.MkSymbol testnamestr, z.MkIntSort())
    let testassertion = z.MkEq(testname, z.MkIntNumeral 3)
    z.AssertCnstr testassertion