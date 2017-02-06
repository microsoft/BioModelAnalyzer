// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
module GenLemmas

//
// Alg 2. Lemma generation procedure GenLemmas.
//

open Microsoft.FSharp.Collections

/// Attempt to find the unique fixpoint of a QN network.
/// But do so lazily, returning a sequence of tighter bounds until the final answer
let stabilize_lazy (network : QN.node list) =

    // The data (name,expr,range) associated with each variable.
    let range = Map.ofList [for node in network -> (node.var, node.range)] 
    let exprs = Map.ofList [for node in network -> (node.var, node.f)]
    let names = Map.ofList [for node in network -> (node.var, node.name)]

    // The inputs to a variable, i.e. a map saying "variable x depends on variables
    // y and z."
    let inputs = Map.ofList [for node in network -> (node.var, node.inputs)]

    // The outputs for each variable, i.e. a map saying "variable x is an input to
    // variables y and z."
    let mutable outputs = Map.ofList [for node in network -> (node.var, Set.empty)]

    for node in network do
        for input in node.inputs do
            let curr_outs = outputs.[input]
            outputs <- Map.add input (curr_outs.Add node.var) outputs

    //let mutable bounds = Map.ofList [for node in network -> (node.var, (Map.find node.var range))]
    let mutable bounds = 
        Map.ofList 
            (List.fold 
                (fun bb (n:QN.node) -> 
                    if (Expr.is_a_const range n.f) then 
                        let c = Expr.eval_expr n.var range n.f Map.empty
                        (n.var,(c,c)) :: bb
                    else (n.var, Map.find n.var range) :: bb)
                []
                network)
    //BH test here to see if we have already got a singleton
    if Map.isEmpty (Map.filter (fun _ (bottom,top) -> bottom <> top) bounds) then ([(false,bounds)]|>Seq.ofList) else
        // The frontier set - variables that have had their bounds updated recently.
        // Initially, all variables are in the frontier (with no reason). 
        let mutable frontier = 
            Set.ofList 
                (List.fold 
                    (fun bb (n:QN.node) -> 
                        if (Expr.is_a_const range n.f) then bb
                        else n.var :: bb)
                    []
                    network)
   
        // initial state for Seq.unfold 
        let still_working = not (Set.isEmpty frontier) 
        let initial_state = (still_working,frontier,bounds,outputs)
    
        // The main loop - keep tightening bounds until we can't tighten any further.
        Seq.unfold 
           (fun (still_working,frontier,bounds,outputs) ->
                if still_working then
                    if (not (Set.isEmpty frontier)) then                
                        // Arbitrarily pick the lowest node in the frontier.
                        let node = Set.maxElement frontier // SI: Garvit's max, used to be min. 
                        let frontier = Set.remove node frontier
                        let expr = Map.find node exprs
                        let (lower, upper) = Map.find node bounds

                        let (new_lower, new_upper) = FNewLemmas.tighten node range expr lower upper (Map.find node inputs) bounds

                        // If we were able to tighten the bound on this variable, then add keep a note of the new bounds,
                        // and add any dependent variables to the frontier. 
                        let bounds,frontier = 
                            if new_upper < upper || new_lower > lower then
                                let bounds' = Map.add node (new_lower, new_upper) (Map.remove node bounds)
                                let frontier' = Set.fold (fun fr o -> Set.add o fr) frontier (Map.find node outputs)
                                Log.log_debug ("Tightened var(" + (string)node + "). Adding it's outputs to frontier")
                                (bounds',frontier')
                            else bounds, frontier
                        Some ((true,bounds),(true,frontier,bounds,outputs))

                    // We've tightened our bounds as much as we can - either every variable has a singleton
                    // bound now (i.e. it's stabilized) or there's at least one variable with a non-constant
                    // bound, in which case we haven't proved anything.
                    else
                        Some ((false,bounds),(false,frontier,bounds,outputs))
                    //          ^                           indicate to client that this is the last proof step
                    //                         ^ indicate to next Seq.unfold that we're done
                else 
                    None)
            initial_state

/// Attempt to find the unique fixpoint of a QN network.
/// But do so lazily, returning a sequence of tighter bounds until the final answer
let stabilize_lazy_1 (network : QN.node list) =

    // The data (name,expr,range) associated with each variable.
    let range = Map.ofList [for node in network -> (node.var, node.range)] 
    let exprs = Map.ofList [for node in network -> (node.var, node.f)]
    let names = Map.ofList [for node in network -> (node.var, node.name)]

    // The inputs to a variable, i.e. a map saying "variable x depends on variables
    // y and z."
    let inputs = Map.ofList [for node in network -> (node.var, node.inputs)]

    // The outputs for each variable, i.e. a map saying "variable x is an input to
    // variables y and z."
    let mutable outputs = Map.ofList [for node in network -> (node.var, Set.empty)]

    for node in network do
        for input in node.inputs do
            let curr_outs = outputs.[input]
            outputs <- Map.add input (curr_outs.Add node.var) outputs

    //let mutable bounds = Map.ofList [for node in network -> (node.var, (Map.find node.var range))]
    let bounds = 
        Map.ofList 
            (List.fold 
                (fun bb (n:QN.node) -> 
                    if (Expr.is_a_const range n.f) then 
                        let c = Expr.eval_expr n.var range n.f Map.empty
                        (n.var,(c,c)) :: bb
                    else (n.var, Map.find n.var range) :: bb)
                []
                network)

    // The frontier set - variables that have had their bounds updated recently.
    // Initially, all variables are in the frontier (with no reason). 
    let frontier = 
        Set.ofList 
            (List.fold 
                (fun bb (n:QN.node) -> 
                    if (Expr.is_a_const range n.f) then bb
                    else n.var :: bb)
                []
                network)
   
    let rec produce_bounds frontier bounds =
        if Set.isEmpty frontier then [(false, bounds)]
        else
            let node = Set.maxElement frontier
            let frontier = Set.remove node frontier
            let expr = Map.find node exprs
            let (lower, upper) = Map.find node bounds
            let (new_lower, new_upper) = FNewLemmas.tighten node range expr lower upper (Map.find node inputs) bounds

            if new_upper < upper || new_lower > lower then
                let new_bounds = Map.add node (new_lower, new_upper) (Map.remove node bounds)
                let new_frontier = Set.fold (fun fr o -> Set.add o fr) frontier (Map.find node outputs)
                Log.log_debug ("Tightened var(" + (string)node + "). Adding it's outputs to frontier")
                (true, bounds)::produce_bounds new_frontier new_bounds
            else
                produce_bounds frontier bounds 


    produce_bounds frontier bounds
    

/// Attempt to find the unique fixpoint of a QN network.
/// But do so lazily, returning a sequence of tighter bounds until the final answer
let stabilize_lazy_2 (network : QN.node list) =

    // The data (name,expr,range) associated with each variable.
    let range = Map.ofList [for node in network -> (node.var, node.range)] 
    let exprs = Map.ofList [for node in network -> (node.var, node.f)]
    let names = Map.ofList [for node in network -> (node.var, node.name)]

    // The inputs to a variable, i.e. a map saying "variable x depends on variables
    // y and z."
    let inputs = Map.ofList [for node in network -> (node.var, node.inputs)]

    // The outputs for each variable, i.e. a map saying "variable x is an input to
    // variables y and z."
    let mutable outputs = Map.ofList [for node in network -> (node.var, Set.empty)]

    for node in network do
        for input in node.inputs do
            let curr_outs = outputs.[input]
            outputs <- Map.add input (curr_outs.Add node.var) outputs

    //let mutable bounds = Map.ofList [for node in network -> (node.var, (Map.find node.var range))]
    let bounds = 
        Map.ofList 
            (List.fold 
                (fun bb (n:QN.node) -> 
                    if (Expr.is_a_const range n.f) then 
                        let c = Expr.eval_expr n.var range n.f Map.empty
                        (n.var,(c,c)) :: bb
                    else (n.var, Map.find n.var range) :: bb)
                []
                network)

    // The frontier set - variables that have had their bounds updated recently.
    // Initially, all variables are in the frontier (with no reason). 
    let frontier = 
        Set.ofList 
            (List.fold 
                (fun bb (n:QN.node) -> 
                    if (Expr.is_a_const range n.f) then bb
                    else n.var :: bb)
                []
                network)
   
    let rec produce_bounds frontier bounds =
        let mutable new_bounds = bounds
        let mutable new_frontier = Set.empty
        let mutable found = false
        for node in frontier do 
            let expr = Map.find node exprs
            let (lower, upper) = Map.find node bounds
            let (new_lower, new_upper) = FNewLemmas.tighten node range expr lower upper (Map.find node inputs) bounds

            if new_upper < upper || new_lower > lower then
                found <- true
                new_bounds <- Map.add node (new_lower, new_upper) (Map.remove node new_bounds)
                new_frontier <- Set.fold (fun fr o -> Set.add o fr) new_frontier (Map.find node outputs)
                Log.log_debug ("Tightened var(" + (string)node + "). Adding it's outputs to frontier")

        if (found) then
            (true, bounds)::produce_bounds new_frontier new_bounds
        else
            [(false, bounds)]

    produce_bounds frontier bounds
    
        
