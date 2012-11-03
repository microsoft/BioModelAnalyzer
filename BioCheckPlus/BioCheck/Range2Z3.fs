(* Copyright (c) Microsoft Corporation. All rights reserved. *)

//#light

//#I "c:/Program Files (x86)/Microsoft Research/bin"
//#r @"Microsoft.Z3.dll"


module Range2Z3

// Pass the "network" and "nuRangel" to the following function, 
// according to the returned resulting list of ranges, and the 
// initial value of k, denoted as "initK", encode the system in 
// Z3 format in initK steps.

open Microsoft.Z3

let ObtainAssertionForOneStep currentRange stepUnrolled (z : Context) = 
    let mutable varsToBeHandled = currentRange
    let mutable numOfVarHandled = 0
    let mutable assertionForOneStep = z.MkTrue()
    while not varsToBeHandled.IsEmpty do
        let mutable currentVariable = Map.find numOfVarHandled varsToBeHandled
        //Then, since we have the first possible range for the first variable v_0 at time t
        // equals to 0, we first assign the name for the variable consistently, and then
        // assert corresponding Z3 assertion.
        let mutable nameOfCurVarAtCurTime = sprintf "v^%d^%d" numOfVarHandled stepUnrolled
        let mutable minVar = List.min currentVariable
        let mutable maxVar = List.max currentVariable          
        //assert Z3 assertion now, one for one variable at one time step
        let mutable currentVar = z.MkConst(z.MkSymbol nameOfCurVarAtCurTime, z.MkIntSort())
        let mutable assertionForOneVarAtOneStep = z.MkAnd(z.MkGe(currentVar, z.MkIntNumeral minVar), z.MkLe(currentVar, z.MkIntNumeral maxVar))
        let mutable assertionForOneStep = z.MkAnd(assertionForOneStep, assertionForOneVarAtOneStep)
        varsToBeHandled <- varsToBeHandled.Remove numOfVarHandled
        numOfVarHandled <- numOfVarHandled + 1
    assertionForOneStep

let ObtainAssertionForKSteps pathsToBeHandled (z : Context) =
    let mutable stepUnrolled = 0
    let mutable assertionForKSteps = z.MkTrue()
    
    while not pathsToBeHandled.IsEmpty do
        let mutable currentRange = pathsToBeHandled.Head
        let mutable assertionForOneStep = ObtainAssertionForOneStep currentRange stepUnrolled z
        let mutable assertionForKSteps = z.MkAnd(assertionForKSteps, assertionForOneStep)
        pathsToBeHandled <- pathsToBeHandled.Tail
        stepUnrolled <- stepUnrolled + 1
    assertionForKSteps

let Z3forSys network initBound (z : Context) =
    
    let (paths, initK) = Paths.OutputPaths network initBound
    // construct the |[M]|_initk, given the paths and initk
    let mutable pathsToBeHandled = paths
    let AssertionsForSys = ObtainAssertionForKSteps pathsToBeHandled z
    
    z.AssertCnstr AssertionsForSys