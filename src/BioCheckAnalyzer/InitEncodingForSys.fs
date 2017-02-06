// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
module InitEncodingForSys

// Pass the "network" and "nuRangel" to the following function, 
// according to the returned resulting list of ranges, and the 
// initial value of k, denoted as "initK", encode the system in 
// Z3 format in initK steps.

open Microsoft.Z3


let initZ3forSys network initBound (paths : Map<QN.var,int list> list) initK (z : Context) =
    
    use s = z.MkSolver()

    let boolean_encoding = true
    
    //abstract the last range from the paths
    let initRange = List.head(paths)
    let fixedRange = List.head(List.rev(paths))

    // construct the |[M]|_initk, given the paths and initk
    //let allVarName = Z.build_var_name_map network

    // the encoding for states 0..(initK-1)
    let mutable pathsToBeHandled = paths @ [fixedRange]
    // let mutable pathsToBeHandled = Map.map (fun x y -> Const(y)) pathsToBeHandled_ints
        
    let mutable stepUnrolled = 0
    let mutable previousRange = Map.empty
    while not pathsToBeHandled.IsEmpty do
        let mutable currentRange = List.head(pathsToBeHandled)
        for node in network do
            if boolean_encoding then
                let list_of_bool_vars = BioCheckPlusZ3.allocate_bool_vars node (Map.find node.var currentRange) stepUnrolled z
                ignore(BioCheckPlusZ3.create_implication_for_bool_vars list_of_bool_vars z)
            else
                ignore(BioCheckZ3.assert_bound node ((List.min(Map.find node.var currentRange)) , (List.max(Map.find node.var currentRange))) stepUnrolled z)

            // Create list of inputs excluding the node itself
            let nodeinputlist =
                List.concat
                    [ for var in node.inputs do
                        yield (List.filter (fun (x:QN.node) -> ((x.var = var) && not (x.var = node.var))) network) ]

            // Add the node itself as head of list
            let nodelist = node :: nodeinputlist
            if boolean_encoding then
                if not previousRange.IsEmpty then 
                    let target_function_bool_constraint = BioCheckPlusZ3.constraint_for_target_function_boolean network node nodeinputlist (Map.find node.var currentRange) previousRange stepUnrolled (stepUnrolled - 1) z s
                    s.Assert target_function_bool_constraint
//                    ignore(BioCheckPlusZ3.assert_target_function_boolean network node nodeinputlist (Map.find node.var currentRange) previousRange stepUnrolled (stepUnrolled - 1) z)
            else
                let varnamenode = BioCheckZ3.build_var_name_map nodelist
                stepZ3rangelist.assert_target_function network node varnamenode initRange stepUnrolled (stepUnrolled + 1) z s

        previousRange <- currentRange
        pathsToBeHandled <- pathsToBeHandled.Tail
        stepUnrolled <- stepUnrolled + 1


        s.Push()
        let sat = s.Check()
        printf "%A" sat
        s.Pop()

// In order to keep the naming of states to be consistent with the one of transitions for system, we can just use the above way simply.
// save if the way used is not correct...

//    let mutable pathsToBeHandled = paths
//    let mutable stepUnrolled = 0
//    
//    
//    while not pathsToBeHandled.IsEmpty do
//        let mutable currentRange = pathsToBeHandled.Head
//        let mutable varsToBeHandled = currentRange
//        let mutable numOfVarHandled = 0
//        
//        while not varsToBeHandled.IsEmpty do
//            let mutable currentVariable = Map.find numOfVarHandled varsToBeHandled
//            // numOfVarHandled is the QN.var for the node in network
//            let currNodeVar = numOfVarHandled
//            // find out the corresponding QN.name for this node
//            let mutable currentNode = List.find (fun (x:QN.node) -> x.var = currNodeVar) network
//            //Then, since we have the first possible range for the first variable v_0 at time t
//            // equals to 0, we first assign the name for the variable consistently, and then
//            // assert corresponding Z3 assertion.
//            let mutable nameOfCurVarAtCurTime = sprintf "%s^%d" currentNode.name stepUnrolled
//            let mutable minVar = List.min currentVariable
//            let mutable maxVar = List.max currentVariable          
//            //assert Z3 assertion now, one for one variable at one time step
//            let mutable currentVar = z.MkConst(z.MkSymbol nameOfCurVarAtCurTime, z.MkIntSort())
//            let mutable assertionForOneVarAtOneStep = z.MkAnd(z.MkGe(currentVar, z.MkIntNumeral minVar), z.MkLe(currentVar, z.MkIntNumeral maxVar))
//            s.Assert assertionForOneVarAtOneStep
//            varsToBeHandled <- varsToBeHandled.Remove numOfVarHandled
//            numOfVarHandled <- numOfVarHandled + 1
//                
//        pathsToBeHandled <- pathsToBeHandled.Tail
//        stepUnrolled <- stepUnrolled + 1

    

    
    fixedRange


type VariableRange = Map<QN.var, int list>
type QN = QN.node list

let encode_current_range network (currentRange : VariableRange) stepUnrolled (z : Context) (s : Solver) =
    for node in network do
        let list_of_bool_vars = BioCheckPlusZ3.allocate_bool_vars node (Map.find node.var currentRange) stepUnrolled z
        BioCheckPlusZ3.create_implication_for_bool_vars list_of_bool_vars z s


// Note that the code in this function duplicates the code of function
// encode_transition_with_time
let encode_transition (network:QN) (previous_range : VariableRange) (current_range : VariableRange) current_step (z : Context) (s : Solver) =
    if not previous_range.IsEmpty then 
        for node in network do
            let node_values = Map.find node.var current_range
            if not (List.isEmpty node_values) then 
                // Create list of inputs excluding the node itself
                let nodeinputlist =
                    List.concat
                        [ for var in node.inputs do
                            yield (List.filter (fun (x:QN.node) -> ((x.var = var) && not (x.var = node.var))) network) ]

                // Add the node itself as head of list
                let inputs_including_node = node :: nodeinputlist
                let transition_constraint = 
                    BioCheckPlusZ3.constraint_for_target_function_boolean network node inputs_including_node (Map.find node.var current_range) previous_range current_step (current_step - 1) z s
                s.Assert transition_constraint

// Note that the code in this function duplicates the code of function
// encode_transition
let encode_transition_with_loop (network:QN) (last_range : VariableRange) (current_range : VariableRange) last_step current_step (z : Context) (s : Solver) =
    for node in network do
        let node_values = Map.find node.var current_range
        if not (List.isEmpty node_values) then
            let nodeinputlist =
                List.concat [ for var in node.inputs do
                                yield (List.filter (fun (x:QN.node) -> ((x.var = var) && not (x.var = node.var))) network) ]
                                
            let inputs_including_node = node :: nodeinputlist
            let z3_constraint_for_trans = BioCheckPlusZ3.constraint_for_target_function_boolean network node inputs_including_node (Map.find node.var current_range) last_range current_step last_step z s
            let z3_constraint_for_loop_at_time = BioCheckPlusZ3.constraint_for_loop_at_time current_step last_step z
            (s.Assert(z.MkImplies(z3_constraint_for_loop_at_time,z3_constraint_for_trans)))


/// Encode the path 
let encode_boolean_paths network (z : Context, s : Solver) (ranges : VariableRange list) =

    // Encode each range, and the transition from it's previous range
    let time = ref 0 
    let previous_range= ref Map.empty

    for range in ranges do

        encode_current_range network range !time z s
        encode_transition network !previous_range range !time z s

        incr time
        previous_range := range

    ()


// Encode the loop closure
// if loop_closure_loop is in the range 0 .. ranges.Length-1 then it indicates
// the exact position of loop closure and only the constraint for that position needs to be 
// asserted
let encode_loop_closure network (z : Context, s : Solver) (ranges : VariableRange list) (loop_closure_loc : int) =
    let last_time = ranges.Length - 1 
    let last_range = List.head (List.rev ranges)

    let time = ref 0
    for range in ranges do
        if !time = loop_closure_loc || loop_closure_loc < 0 || loop_closure_loc > last_time then
            encode_transition_with_loop network last_range range last_time !time z s

        incr time

    ()

// In order for the automaton and the paths to agree on the same
// location of the loop we allocate Boolean variables that encode
// in unary the location of the loop closure.
// The path encoded has length time points
// We allocated length-1 Boolean variables corresponding to
// times 0,1,2,....,length-2 (ignoring time length-1).
// We add an implication that v0->v1, v1->v2, ...
// The possible assignments are:
// 11...11
// 01...11
// 001..11
// ...
// 00..011
// 00...01
// 00...00
// The loop closes at time 0 if v0 (and length >= 2)
// The loop closes at time 1 if !v0 & v1 
// The loop closes at time i if !vi-1 & vi
// The loop closes at time n-1 if !vn-2
//
// loop_closure_loop indicates the exact position of loop closure (-1 for leave open)
let encode_loop_closure_variables (z : Context, s : Solver) length loop_closure_loc =
    let list_of_variables = BioCheckPlusZ3.allocate_loop_vars z length
    if (loop_closure_loc < 0 || loop_closure_loc >= length) then
        BioCheckPlusZ3.create_implication_for_bool_vars list_of_variables z s
    else
        let list_of_values = [ for i in 0 .. length-2  -> if  i < loop_closure_loc then false else true ]
        BioCheckPlusZ3.assert_values_for_bool_vars list_of_variables list_of_values z s

