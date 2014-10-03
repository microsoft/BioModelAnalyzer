module EncodingForFormula2

open LTL
open QN
open Microsoft.Z3


type VariableRange = Map<QN.var, int list>
type QN = QN.node list
type FormulaLocation = int list 
type FormulaConstraint = Map<FormulaLocation, Term>
type FormulaConstraintList = FormulaConstraint list

let create_z3_bool_var (location : FormulaLocation) time (z: Context) =
    let var_name = BioCheckPlusZ3.get_z3_bool_var_formula_in_location_at_time location time
    BioCheckPlusZ3.make_z3_bool_var var_name z


// Encode [phi] into [z]. As a by-product, return the map of constraints per location. 
// Better name: encode 
// Efficiency: need to memoize xlation. 
let produce_constraints_for_truth_of_formula_at_time (phi : LTLFormulaType) (range : VariableRange) (step : int) (z : Context) : Map<int list,Term> =

    let rec encode_truth_of_formula phi map =
        // SI: encode_> and encode_< must be generalizable? 
        let encode_prop_gt var comparison_function range time =
            if not (List.exists comparison_function range) then
                // There does not exist a value in the range that satisfies the proposition
                z.MkFalse()
            else
                let first_value_satisfying_proposition = List.findIndex comparison_function range
                if (first_value_satisfying_proposition = 0) then
                    // All values in the range satisfy the proposition
                    z.MkTrue()
                else 
                    // Find the last value in the range not satisfying the proposition
                    // If the appropriate z3 variable is false then one of the values satisfying
                    // the proposition must be true
                    let last_value_not_satisfying_prop = List.nth range (first_value_satisfying_proposition-1)
                    let z3_var_name = BioCheckPlusZ3.get_z3_bool_var_at_time_in_val var time last_value_not_satisfying_prop
                    let z3_var = BioCheckPlusZ3.make_z3_bool_var z3_var_name z
                    z.MkNot(z3_var)

        let encode_prop_lt var comparison_function range time =
            if not (List.exists comparison_function range) 
            then
                z.MkTrue()
            else
                let first_value_not_satisfying_proposition = List.findIndex comparison_function range                    
                if (first_value_not_satisfying_proposition = 0)
                then
                    z.MkFalse()
                else
                    let last_value_satisfying_proposition = List.nth range (first_value_not_satisfying_proposition-1)
                    let z3_var_name = BioCheckPlusZ3.get_z3_bool_var_at_time_in_val var time last_value_satisfying_proposition
                    BioCheckPlusZ3.make_z3_bool_var z3_var_name z
        
        match phi with 
        | Until(loc, _, _)
        | Release(loc, _, _) 
        | Next(loc, _) 
        | Always(loc, _) 
        | Eventually(loc, _) -> 
            let c = create_z3_bool_var loc step z
            (c, Map.add loc c map)
        | And(loc,f,g) -> 
            let f',map' = encode_truth_of_formula f map
            let g',map'' = encode_truth_of_formula g map'
            let c = z.MkAnd(f',g')
            (c, Map.add loc c map'')
        | Or(loc,f,g) -> 
            let f',map' = encode_truth_of_formula f map
            let g',map'' = encode_truth_of_formula g map'
            let c = z.MkOr(f',g')
            (c, Map.add loc c map'') 
        | Implies(loc,f,g) -> 
            let f',map' = encode_truth_of_formula f map
            let g',map'' = encode_truth_of_formula g map'
            let c = z.MkImplies(f',g')
            (c, Map.add loc c map'')
        | Not(loc,f) -> 
            let f',map' = encode_truth_of_formula f map
            let c = z.MkNot(f')
            (c, Map.add loc c map')
        | PropGt(loc,var,value) 
        | PropGtEq(loc,var,value) -> 
            let c = encode_prop_gt var (fun n -> n > value) (Map.find var.var range) step
            (c, Map.add loc c map)
        | PropLt(loc,var,value) 
        | PropLtEq(loc,var,value) -> 
            let c = encode_prop_lt var (fun n -> n <= value) (Map.find var.var range) step
            (c, Map.add loc c map)
        | False -> (z.MkFalse(), map)
        | True  -> (z.MkTrue(), map)
        | Error -> failwith "Error"

    let _constraint,loc_to_constraint = encode_truth_of_formula phi Map.empty
    loc_to_constraint

// Get constraints associated with f. 
// Constraints for T,F are tt,ff; otherwise lookup by [loc] in [loc_to_cc].
let cc_of_loc loc_to_cc loc = Map.find loc loc_to_cc 

let cc_of_loc_formula f loc_to_cc (z:Context) = 
    let z3_tt, z3_ff = z.MkTrue(), z.MkFalse() 
    match f with 
    | False -> z3_ff
    | True -> z3_tt
    | Error -> failwith "Error"
    | _ -> match LTL.formula_location f with
            | Some loc -> cc_of_loc loc_to_cc loc 
            | None -> failwith "unreachable"

// Role of additional_application_constraint:
// ==========================================
// When calling this function to encode the transition of the formula from one step to the next (i.e., when step_prev = step_curr-1)
// the constraint on the transition should apply.
// When calling this function to encode the transition of the formula from the last_step to the current_step (i.e., when encoding
// the loop colsure) we should apply constraints only in case that the loop encoding tells us that the loop closes at this point.
// It follows, that there is some additional constraint that only if it holds we should actually assert the constraint created
// by this function.
// This constraint is supplied by the caller.
// In the case of encoding a normal step this constraint should be true --> encode the transition
// In the case of encoding a loop closure this constraint should encode that the loop is closing 
// to this step and only then encode the transition.

// let encode_formula_transition_from_to (ltl_formula : LTLFormulaType) (network : QN) (previous_map : FormulaConstraint) (current_map : FormulaConstraint) (step_prev : int) (step_curr : int) (z : Context) additional_application_constraint = 
let rec encode_transition (phi : LTLFormulaType) (previous_map : FormulaConstraint) (current_map : FormulaConstraint) (step_prev : int) (step_curr : int) (z : Context) assumption = 
   
    // Get constraints associated with [loc]
    // Constraints for T,F are tt,ff. 
    let cc_of_loc loc = cc_of_loc previous_map loc, cc_of_loc current_map loc
    let cc_of_loc_ground_wrapper f = cc_of_loc_formula f previous_map z, cc_of_loc_formula f current_map z 
    
    // Z3 encoding of [phi]
    let t_of_phi = 
        match phi with 
        | Until (loc,f,g) ->
            let loc_prev,loc_curr = cc_of_loc loc 
            let f_prev,  _f_curr  = cc_of_loc_ground_wrapper f
            let g_prev,  _g_curr  = cc_of_loc_ground_wrapper g
            let tt =  z.MkImplies(loc_prev, 
                                  z.MkOr(g_prev, z.MkAnd(f_prev, loc_curr)))         
            let ff = z.MkImplies(z.MkNot loc_prev, 
                                 z.MkAnd(z.MkNot g_prev, z.MkOr(z.MkNot f_prev, z.MkNot loc_curr)))
            z.MkAnd(tt,ff)
        | Release (loc,f,g) ->
            let loc_prev,loc_curr = cc_of_loc loc 
            let f_prev,  _f_curr  = cc_of_loc_ground_wrapper f
            let g_prev,  _g_curr  = cc_of_loc_ground_wrapper g
            let tt = z.MkImplies(loc_prev, 
                                 z.MkOr(z.MkAnd(f_prev, g_prev), 
                                        z.MkAnd(g_prev, loc_curr)))
            let ff = z.MkImplies(z.MkNot loc_prev,
                                 z.MkAnd(z.MkOr(z.MkNot f_prev, z.MkNot g_prev), 
                                         z.MkOr(z.MkNot g_prev, z.MkNot loc_curr)))
            z.MkAnd(tt,ff)
        | Next (loc,f) ->
            let loc_prev, loc_curr = cc_of_loc loc 
            let _f_prev,  f_curr  = cc_of_loc_ground_wrapper f
            let tt = z.MkImplies(loc_prev, f_curr)
            let ff = z.MkImplies(z.MkNot loc_prev, z.MkNot f_curr)
            z.MkAnd(tt,ff)
        | Always (loc,f) ->
            let loc_prev, loc_curr = cc_of_loc loc 
            let f_prev,  _f_curr   = cc_of_loc_ground_wrapper f
            let tt = z.MkImplies(loc_prev,
                                 z.MkAnd(f_prev,loc_curr))
            let ff = z.MkImplies(z.MkNot loc_prev,
                                 z.MkOr(z.MkNot f_prev, z.MkNot loc_curr))
            z.MkAnd(tt,ff) 
        | Eventually (loc,f) ->
            let loc_prev, loc_curr = cc_of_loc loc 
            let f_prev,  _f_curr   = cc_of_loc_ground_wrapper f
            let tt = z.MkImplies(loc_prev,
                                 z.MkOr(f_prev,loc_curr))
            let ff = z.MkImplies(z.MkNot loc_prev,
                                 z.MkAnd(z.MkNot f_prev,z.MkNot loc_curr))
            z.MkAnd(tt,ff) 
        | _ -> z.MkTrue ()
    
    // Take [assumption] into account. 
    let t_of_phi' = 
        match phi with 
        | Until(_)
        | Release(_)
        | Next(_)
        | Always(_)
        | Eventually(_) -> z.MkImplies(assumption,t_of_phi)
        | _ -> t_of_phi

    // assert phi
    z.AssertCnstr t_of_phi' 

    // And then do it all recursively
    match phi with 
    | Until(_,f,g)
    | Release(_,f,g)
    | And(_,f,g) 
    | Or(_,f,g) 
    | Implies(_,f,g) -> 
        encode_transition f previous_map current_map step_prev step_curr z assumption
        encode_transition g previous_map current_map step_prev step_curr z assumption
    | Not(_, f)
    | Next(_, f)
    | Always(_, f)
    | Eventually(_, f) ->
        encode_transition f previous_map current_map step_prev step_curr z assumption
    | _ -> ()



// SI: place this at the beginning of the file (everyone needs the list it produces). 
// return [constrains_for_truth range[0]; constrains_for_truth range[1]; ...]
let create_list_of_maps_of_formula_constraints (f : LTLFormulaType) (z: Context) (ranges : VariableRange list) =
    let (_t,t_to_f_to_constraint_rev) = List.fold 
                                            (fun (t,t_to_f_constraint) range ->
                                                let f_to_constraint = produce_constraints_for_truth_of_formula_at_time f range t z
                                                (t+1, f_to_constraint :: t_to_f_constraint))
                                            (0,[])
                                            ranges 
    List.rev t_to_f_to_constraint_rev


// 
// Entry Points 
// 

let assert_top_most_formula (f : LTLFormulaType) (z : Context) (floc_to_cc : FormulaConstraint) =
    z.AssertCnstr (cc_of_loc_formula f floc_to_cc z)

let encode_formula_transitions_over_path (f : LTLFormulaType) (z : Context) (loc_to_ccs : FormulaConstraintList) =
    let z3_tt = z.MkTrue ()
    let time = 1
    let loc_to_cc_hd, loc_to_cc_tl = 
        match loc_to_ccs with 
        | [] -> failwith "expecting non-[] list" 
        | hd::tl -> hd,tl
    let _ = List.fold 
                (fun (time,previous) current ->
                    let _  = encode_transition f previous current (time-1) time z z3_tt
                    (time+1, current))
                (time,loc_to_cc_hd)
                loc_to_cc_tl
    ()        

let encode_formula_transitions_in_loop_closure (f : LTLFormulaType) (z : Context) (loc_to_ccs : FormulaConstraintList) =
    let last_time = (List.length loc_to_ccs) - 1
    let last_map = 
        match (List.rev loc_to_ccs) with     
        | last :: _ -> last
        | [] -> failwith "non-[] list expected" 

    let time = 0 
    let _ = List.fold   
                (fun time floc_to_cc ->
                    let cnstr = BioCheckPlusZ3.constraint_for_loop_at_time time last_time z 
                    encode_transition f last_map floc_to_cc last_time time z cnstr
                    time + 1)
                time
                loc_to_ccs
    ()



// Go recursively over the formula structure
// For each temporal formula that requires fairness (until, release, always, eventually)
// assert the disjunction that the fairness holds at some point in the loop.
// This is a disjunction of all possible locations saying for each location:
// this is a possible loop location and it is fair.
// In symbols: \/_{time=0}^last_time (inside_loop /\ fair)
// The inside loop predicate is: if time=last_time then it is true
//                               if time<last_time then it is l_time
let encode_formula_loop_fairness (ltl_formula : LTLFormulaType) (z : Context) (formula_constraints : FormulaConstraintList) =
    let last_time = (List.length formula_constraints) - 1

    let inside_loop time =
        if (time = last_time) then z.MkTrue()
        else
            let name_of_loop_var = BioCheckPlusZ3.get_z3_bool_var_loop_at_time time
            BioCheckPlusZ3.make_z3_bool_var name_of_loop_var z
    
    let encode_fairness (phi : LTLFormulaType) = 
        let _t,disjs = List.fold 
                            (fun (time,disjs) formula_constraint ->
                                let inside_loop_cnstr = inside_loop time            
                                let phi_cnstr = cc_of_loc_formula phi formula_constraint z
                                let fair_cnstr = 
                                    match phi with
                                    | Until (_, _, phi0) 
                                    | Eventually (_, phi0) ->
                                        let cnstr_phi0 = cc_of_loc_formula phi0 formula_constraint z
                                        let not_constraint_for_formula = z.MkNot (phi_cnstr)
                                        z.MkOr(z.MkNot phi_cnstr, cnstr_phi0) 
                                    | Release(_, _, phi0) 
                                    | Always(_,phi0) ->
                                        let cnstr = cc_of_loc_formula phi0 formula_constraint z
                                        z.MkOr(phi_cnstr, z.MkNot cnstr)
                                    | _ -> z.MkTrue()
                                let disjunct = z.MkAnd(inside_loop_cnstr,fair_cnstr)
                                (time+1, disjunct::disjs) )
                            (0,[])
                            formula_constraints
        let fairness_cnstr = z.MkOr(List.toArray disjs)
        z.AssertCnstr(fairness_cnstr)

    let rec loop f = 
        match ltl_formula with 
        | Until (_ , g, h) 
        | Release (_, g, h) -> 
            encode_fairness ltl_formula
            loop g; loop h
        | And (_, g, h)
        | Or (_, g, h) ->
            loop g
            loop h
        | Not (_, g)
        | Next (_, g)
        | Always (_, g) -> 
           encode_fairness ltl_formula
           loop g
        | Eventually (_, g) ->
           encode_fairness ltl_formula
           loop g
        |_ -> ()
        
    loop ltl_formula


