﻿(* Copyright (c) Microsoft Corporation. All rights reserved. *)

//#light

//#I "c:/Program Files (x86)/Microsoft Research/bin"
//#r @"Microsoft.Z3.dll"

module ReadTGBA

// given the seq of txt file
// first figure out what are the locations, transitions, and AccSet seprately

// locations are recorded used a string list, where each element represents the ID/name of location.

// transitions are recorded used a string list, where each element is a transition in the automaton.

// Accset is recorded used a string list, where each element is a sub Acc condition.

let LocTransAcc (automatonList: string list) = 
    //let startForLocation = List.findIndex (fun elem -> elem ="Locations") automaton
    //let startForTransition = List.findIndex (fun elem -> elem ="Transtions") automaton
    //let startForAcc = List.findIndex (fun elem -> elem ="AccSet") automaton
    //printf "%d %d %d" startForLocation startForTransition startForAcc
    // the above method costs too much time (add 3 secs to 1 sec)

    let mutable tGBAList = List.tail automatonList
    //let mutable numOfElehandled = 1
    //let mutable locations = Map.empty
    let mutable locations = List.empty
    while (not tGBAList.IsEmpty) && tGBAList.Head <> "Transitions" do
        //locations <- locations.Add(tGBAList.Head, tGBAList.Tail.Head)
        locations <- locations @ [tGBAList.Head]
        tGBAList <- tGBAList.Tail.Tail
    tGBAList <- tGBAList.Tail
    let mutable (transitions:string list) = List.empty
    //let mutable transitions = Map.empty
    while (not tGBAList.IsEmpty) && tGBAList.Head <> "AccSet" do
        // Then, construct the structure of transition as a Map<string, string list list>, 
        // which is (Acc, [["l0"]; ["p"; "q"]; ["l1"]])
        //let mutable currTrans = tGBAList.Head.Split([|';'|])
        //printf "%A" currTrans
        transitions <- List.Cons (tGBAList.Head, transitions)
        tGBAList <- tGBAList.Tail
    tGBAList <- tGBAList.Tail
    let mutable accSet = List.empty
    //let mutable accID = 0
    while not tGBAList.IsEmpty do
        //let mutable currAcc = tGBAList.Head
        //let relatedTrans = List.filter (fun x -> x.Contains(currAcc)) (transitions)
        accSet <- List.Cons (tGBAList.Head, accSet)
        //accID <- accID + 1
        tGBAList <- tGBAList.Tail
    (locations, transitions, accSet)
    //printf "%A" locations
    //printf "%A" transitions
    //printf "%A" accSet
    