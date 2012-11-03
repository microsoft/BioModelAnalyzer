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
    // since the action(s) can be more than one, we need split the conjunctions first
    let mutable actionsToBeHandled = List.filter (fun x -> x <> "") (Array.toList(actionListToBeHandled.Split([|' '|])))
    // all "true" actions can be ignored
    actionsToBeHandled <- List.filter (fun x -> x <> "true") actionsToBeHandled
    
    // then assert the conjunction of all actions
    let mutable assertionOfAllActions = z.MkTrue()
    while not actionsToBeHandled.IsEmpty do
        let mutable action = actionsToBeHandled.Head
        // for each string, it can be a disjunction of more than one action
        // so, split the disjunction first
        let mutable actionToBeHandled = List.filter (fun x -> x <> "") (Array.toList(action.Split([|','|])))
        // then assert the disjunction of all actions first, then the conjunction
        let mutable assertionOfDisjunction = z.MkFalse()
        while not actionToBeHandled.IsEmpty do
            let mutable memInDisj = actionToBeHandled.Head
            let mutable memLength = memInDisj.Length
            let mutable memlengthHandled = 0
            let mutable memnegAct = "false"
            let mutable memactCompList = List.empty
            while memlengthHandled < memLength do 
                let memcompResult =
                    match action.[memlengthHandled] with
                    | '!' -> 
                        memnegAct <- "true"
                        memlengthHandled <- memlengthHandled + 1
                        [memnegAct]

                    | c when (int(c) >= 65 && int(c) <= 90) || (int(c) >= 98 && int(c) <= 122) -> // a..z A..Z
                        let mutable nameLength = 1
                        let mutable nextToken = action.[(memlengthHandled + nameLength)]
                        while not (nextToken = '=') &&  not (nextToken = '>') &&  not (nextToken = '<') do
                             nameLength <- nameLength + 1
                             nextToken <- action.[(memlengthHandled + nameLength)]
                        let memsymOfAct = sprintf "%s^%d" action.[memlengthHandled..(memlengthHandled + nameLength - 1)] currStep
                        memlengthHandled <- memlengthHandled + nameLength
                        [memsymOfAct]

                    | '=' | '>' | '<' ->
                        let memcompResulttmp =
                            match action.[memlengthHandled+1] with
                            | '=' -> 
                                let memoperator = action.[memlengthHandled..(memlengthHandled+1)]
                                memlengthHandled <- memlengthHandled + 2
                                memoperator
                            | _ -> 
                                let memoperator = action.[memlengthHandled..memlengthHandled]
                                memlengthHandled <- memlengthHandled + 1
                                memoperator
                        [memcompResulttmp]
                    | _ -> //where the remaining char(s) is(are) a int
                        let memcomparedValue = action.[memlengthHandled..(memLength-1)]
                        memlengthHandled <- memLength
                        [memcomparedValue]
                memactCompList <- memactCompList @ memcompResult
            actionToBeHandled <- actionToBeHandled.Tail
            
            let memnumOfComps = memactCompList.Length
            if memnumOfComps = 4 then
                let memnameOfAct = z.MkConst(z.MkSymbol memactCompList.Tail.Head, z.MkIntSort())
                let memactOperator = memactCompList.Tail.Tail.Head
                let memvalue = int(memactCompList.Tail.Tail.Tail.Head)
                let assertForOneMemAct = 
                    match memactOperator with
                    | "=" -> z.MkNot(z.MkEq(memnameOfAct, z.MkIntNumeral memvalue))
                    | ">" -> z.MkNot(z.MkGt(memnameOfAct, z.MkIntNumeral memvalue))
                    | "<" -> z.MkNot(z.MkLt(memnameOfAct, z.MkIntNumeral memvalue))
                    | ">=" -> z.MkNot(z.MkGe(memnameOfAct, z.MkIntNumeral memvalue))
                    | _ -> z.MkNot(z.MkLe(memnameOfAct, z.MkIntNumeral memvalue))
                assertionOfDisjunction <- z.MkOr(assertionOfDisjunction, assertForOneMemAct)        
            else 
                let memnameOfAct = z.MkConst(z.MkSymbol memactCompList.Head, z.MkIntSort())
                let memactOperator = memactCompList.Tail.Head
                let memvalue = int(memactCompList.Tail.Tail.Head)
                let assertForOneMemAct = 
                    match memactOperator with
                    | "=" -> z.MkNot(z.MkEq(memnameOfAct, z.MkIntNumeral memvalue))
                    | ">" -> z.MkNot(z.MkGt(memnameOfAct, z.MkIntNumeral memvalue))
                    | "<" -> z.MkNot(z.MkLt(memnameOfAct, z.MkIntNumeral memvalue))
                    | ">=" -> z.MkNot(z.MkGe(memnameOfAct, z.MkIntNumeral memvalue))
                    | _ -> z.MkNot(z.MkLe(memnameOfAct, z.MkIntNumeral memvalue))
                assertionOfDisjunction <- z.MkOr(assertionOfDisjunction, assertForOneMemAct)
             
            //printfn "%s" "done"
        actionsToBeHandled <- actionsToBeHandled.Tail
        assertionOfAllActions <- z.MkAnd(assertionOfAllActions, assertionOfDisjunction)
//        let mutable actlength = action.length
//        
//        let mutable lengthhandled = 0
//        let mutable negact = "false"
//        let mutable actcomplist = list.empty
//        while lengthhandled < actlength do 
//            let compresult =
//                match action.[lengthhandled] with
//                | '!' -> 
//                    negact <- "true"
//                    lengthhandled <- lengthhandled + 1
//                    [negact]
////                | 'v' -> // could be any string as node.name
////                    let symofact = sprintf "%s^%d" action.[lengthhandled..(lengthhandled+2)] currstep
////                    lengthhandled <- lengthhandled + 3
////                    [symofact]
//                | c when (int(c) >= 65 && int(c) <= 90) || (int(c) >= 98 && int(c) <= 122) -> // a..z a..z
//                    let mutable namelength = 1
//                    let mutable nexttoken = action.[(lengthhandled + namelength)]
//                    while not (nexttoken = '=') &&  not (nexttoken = '>') &&  not (nexttoken = '<') do
//                         namelength <- namelength + 1
//                         nexttoken <- action.[(lengthhandled + namelength)]
//                    let symofact = sprintf "%s^%d" action.[lengthhandled..(lengthhandled + namelength - 1)] currstep
//                    lengthhandled <- lengthhandled + namelength
//                    [symofact]
//
//                | '=' | '>' | '<' ->
//                    let compresulttmp =
//                        match action.[lengthhandled+1] with
//                        | '=' -> 
//                            let operator = action.[lengthhandled..(lengthhandled+1)]
//                            lengthhandled <- lengthhandled + 2
//                            operator
//                        | _ -> 
//                            let operator = action.[lengthhandled..lengthhandled]
//                            lengthhandled <- lengthhandled + 1
//                            operator
//                    [compresulttmp]
//                | _ -> //where the remaining char(s) is(are) a int
//                    let comparedvalue = action.[lengthhandled..(actlength-1)]
//                    lengthhandled <- actlength
//                    [comparedvalue]
//            actcomplist <- actcomplist @ compresult
//        actionstobehandled <- actionstobehandled.tail
//        //printf "%a" actcomplist
//        let numofcomps = actcomplist.length
//        if numofcomps = 4 then
//            let nameofact = z.mkconst(z.mksymbol actcomplist.tail.head, z.mkintsort())
//            let actoperator = actcomplist.tail.tail.head
//            let value = int(actcomplist.tail.tail.tail.head)
//            let assertforoneact = 
//                match actoperator with
//                | "=" -> z.mknot(z.mkeq(nameofact, z.mkintnumeral value))
//                | ">" -> z.mknot(z.mkgt(nameofact, z.mkintnumeral value))
//                | "<" -> z.mknot(z.mklt(nameofact, z.mkintnumeral value))
//                | ">=" -> z.mknot(z.mkge(nameofact, z.mkintnumeral value))
//                | _ -> z.mknot(z.mkle(nameofact, z.mkintnumeral value))
//            assertionofallactions <- z.mkand(assertionofallactions, assertforoneact)        
//        else 
//            let nameofact = z.mkconst(z.mksymbol actcomplist.head, z.mkintsort())
//            let actoperator = actcomplist.tail.head
//            let value = int(actcomplist.tail.tail.head)
//            let assertforoneact = 
//                match actoperator with
//                | "=" -> z.mknot(z.mkeq(nameofact, z.mkintnumeral value))
//                | ">" -> z.mknot(z.mkgt(nameofact, z.mkintnumeral value))
//                | "<" -> z.mknot(z.mklt(nameofact, z.mkintnumeral value))
//                | ">=" -> z.mknot(z.mkge(nameofact, z.mkintnumeral value))
//                | _ -> z.mknot(z.mkle(nameofact, z.mkintnumeral value))
//            assertionofallactions <- z.mkand(assertionofallactions, assertforoneact)        
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
        
            





    