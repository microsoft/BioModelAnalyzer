(* Copyright (c) Microsoft Corporation. All rights reserved. *)

//#light

//#I "c:/Program Files (x86)/Microsoft Research/bin"
//#r @"Microsoft.Z3.dll"

module TGBATransEncoding
//given the list of TGBA transitions,
//construct the transition Z3 encoding for each step (say from step h to step h+1),
//first classify each element of the list (a trans) into four parts: start loc, action(s), acc(which can be ignored here),
//and end loc.
//for the loc, use "l^0^0" to indicate the loc id and time step
//for the action(s), analyse first and end up with sth.like /\(a<=v^i^j<=b)

open Microsoft.Z3

let assetInitLoc (locations:string list) (z:Context)=
    let numOfLocs = locations.Length
    // l0^0 is true denoted as 1, and other locs are false denoted as 0
    let nameOfL0 = z.MkConst(z.MkSymbol "l0^0", z.MkIntSort())
    let assertionL0 = z.MkEq(nameOfL0, z.MkIntNumeral 1)
    z.AssertCnstr assertionL0
    for i in [1..(numOfLocs - 1)] do
        let symOfLi = sprintf "l%d^0" i
        let nameOfLi = z.MkConst(z.MkSymbol symOfLi, z.MkIntSort())
        let assertionLi = z.MkEq(nameOfLi, z.MkIntNumeral 0)
        z.AssertCnstr assertionLi

let ClassifyTrans (transitionList:string list) =
    let splitedTrans =
        [for trans in transitionList do
            yield Array.toList(trans.Split([|';'|]))]
    let accDropedTrans =
        [for classifiedTrans in splitedTrans do
            yield classifiedTrans.Head::classifiedTrans.Tail.Head::classifiedTrans.Tail.Tail.Tail]
    accDropedTrans




let assertOneTransAtOneStep (trans:string list) (currStep:int) (z:Context) =
    //return a Z3 term indicating startLoc /\ actions /\ endLoc
    //assert the start location first
    let symOfStartLoc = sprintf "%s^%d" trans.Head currStep
   // printf "%s" trans.Head
    let nameOfStartLoc = z.MkConst(z.MkSymbol symOfStartLoc, z.MkIntSort())
    let assertionStartLoc = z.MkEq(nameOfStartLoc, z.MkIntNumeral 1)
    //z.AssertCnstr assertionStartLoc

    //assert the end location then
    let symOfEndLoc = sprintf "%s^%d" trans.Tail.Tail.Head (currStep+1)
    let nameOfEndLoc = z.MkConst(z.MkSymbol symOfEndLoc, z.MkIntSort())
    let assertionEndLoc = z.MkEq(nameOfEndLoc, z.MkIntNumeral 1)
    //z.AssertCnstr assertionEndLoc

    //assert the actions finally
    let actionListToBeHandled = trans.Tail.Head
    // since the action(s) can be more than one, we need split them first
    let mutable actionsToBeHandled = List.filter (fun x -> x <> "") (Array.toList(actionListToBeHandled.Split([|' '|])))
    // all "true" actions can be ignored
    actionsToBeHandled <- List.filter (fun x -> x <> "true") actionsToBeHandled
    // then assert the conjunction of all actions
    let mutable assertionOfAllActions = z.MkTrue()
    while not actionsToBeHandled.IsEmpty do
        let mutable action = actionsToBeHandled.Head
        let mutable actLength = action.Length
        
        let mutable lengthHandled = 0
        let mutable negAct = "false"
        let mutable actCompList = List.empty
        while lengthHandled < actLength do 
            let compResult =
                match action.[lengthHandled] with
                | '!' -> 
                    negAct <- "true"
                    lengthHandled <- lengthHandled + 1
                    [negAct]
//                | 'v' -> // could be any string as Node.name
//                    let symOfAct = sprintf "%s^%d" action.[lengthHandled..(lengthHandled+2)] currStep
//                    lengthHandled <- lengthHandled + 3
//                    [symOfAct]
                | c when (int(c) >= 65 && int(c) <= 90) || (int(c) >= 98 && int(c) <= 122) -> // a..z A..Z
                    let mutable nameLength = 1
                    let mutable nextToken = action.[(lengthHandled + nameLength)]
                    while not (nextToken = '=') &&  not (nextToken = '>') &&  not (nextToken = '<') do
                         nameLength <- nameLength + 1
                         nextToken <- action.[(lengthHandled + nameLength)]
                    let symOfAct = sprintf "%s^%d" action.[lengthHandled..(lengthHandled + nameLength - 1)] currStep
                    lengthHandled <- lengthHandled + nameLength
                    [symOfAct]

                | '=' | '>' | '<' ->
                    let compResulttmp =
                        match action.[lengthHandled+1] with
                        | '=' -> 
                            let operator = action.[lengthHandled..(lengthHandled+1)]
                            lengthHandled <- lengthHandled + 2
                            operator
                        | _ -> 
                            let operator = action.[lengthHandled..lengthHandled]
                            lengthHandled <- lengthHandled + 1
                            operator
                    [compResulttmp]
                | _ -> //where the remaining char(s) is(are) a int
                    let comparedValue = action.[lengthHandled..(actLength-1)]
                    lengthHandled <- actLength
                    [comparedValue]
            actCompList <- actCompList @ compResult
        actionsToBeHandled <- actionsToBeHandled.Tail
        //printf "%A" actCompList
        let numOfComps = actCompList.Length
        if numOfComps = 4 then
            let nameOfAct = z.MkConst(z.MkSymbol actCompList.Tail.Head, z.MkIntSort())
            let actOperator = actCompList.Tail.Tail.Head
            let value = int(actCompList.Tail.Tail.Tail.Head)
            let assertForOneAct = 
                match actOperator with
                | "=" -> z.MkNot(z.MkEq(nameOfAct, z.MkIntNumeral value))
                | ">" -> z.MkNot(z.MkGt(nameOfAct, z.MkIntNumeral value))
                | "<" -> z.MkNot(z.MkLt(nameOfAct, z.MkIntNumeral value))
                | ">=" -> z.MkNot(z.MkGe(nameOfAct, z.MkIntNumeral value))
                | _ -> z.MkNot(z.MkLe(nameOfAct, z.MkIntNumeral value))
            assertionOfAllActions <- z.MkAnd(assertionOfAllActions, assertForOneAct)        
        else 
            let nameOfAct = z.MkConst(z.MkSymbol actCompList.Head, z.MkIntSort())
            let actOperator = actCompList.Tail.Head
            let value = int(actCompList.Tail.Tail.Head)
            let assertForOneAct = 
                match actOperator with
                | "=" -> z.MkNot(z.MkEq(nameOfAct, z.MkIntNumeral value))
                | ">" -> z.MkNot(z.MkGt(nameOfAct, z.MkIntNumeral value))
                | "<" -> z.MkNot(z.MkLt(nameOfAct, z.MkIntNumeral value))
                | ">=" -> z.MkNot(z.MkGe(nameOfAct, z.MkIntNumeral value))
                | _ -> z.MkNot(z.MkLe(nameOfAct, z.MkIntNumeral value))
            assertionOfAllActions <- z.MkAnd(assertionOfAllActions, assertForOneAct)        
    assertionOfAllActions

        
            
        
    
    

let assertTransAtOneStep (accDropedTrans:string list list) (currStep:int) (z:Context) =
    let mutable assertionForAllTrans = z.MkFalse()
    let mutable transListToBeHandled = accDropedTrans
    while not transListToBeHandled.IsEmpty do
        let mutable currTrans = transListToBeHandled.Head
        let mutable currAssertion = assertOneTransAtOneStep currTrans currStep z
        assertionForAllTrans <- z.MkOr(assertionForAllTrans, currAssertion)
        transListToBeHandled <- transListToBeHandled.Tail
    assertionForAllTrans
        
            





    