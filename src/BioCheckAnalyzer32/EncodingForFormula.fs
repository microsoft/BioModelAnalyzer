
module EncodingForFormula

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


// Inefficient:
// If the same formula appears twice in different positions (particularly true for propositions!)
// then it is encoded twice!
let produce_constraints_for_truth_of_formula_at_time (ltl_formula : LTLFormulaType) (network : QN) (range : VariableRange) (step : int) (z : Context) =
    let return_map = ref Map.empty
    let rec encode_truth_of_formula ltl_formula =
        // Encoding
        // ========
        // The encoding of values of variables is as follows.
        // There is a list of possible values (in prev_range or current_range)
        // The variable is allocated |possible values|-1 boolean variables named according
        // to the possible values
        // v_{i_0} is true, then the value is i_0
        // v_{i_{j-1}}is false and v_{i_j} is true then the value is i_j
        // v_{i_{n-2}} is false then the value is i_{n-1}
        //
        // For GT type propositions
        // ========================
        // If all values do not satisfy the proposition then false
        // If all values satisfy the proposition then true
        // Otherwise, find the maximal value that does not satisfy the proposition
        // and require that it be false (so one of the other values larger than it
        // must be the encoded value)
        //
        // For LT type propositions
        // ========================
        // If all values satisfy the proposition then true
        // If all values do not satisfy the proposition then false
        // Otherwise, find the maximal value that does satisfy the proposition
        // and requre that it be true (so either it or one of the values smaller than
        // it must be the encoded value)
        //
        // For = type propositions
        // =======================
        // If all the values do not satisfy the proposition then false
        // If all the values satisfy the proposition then true
        // Otherise, require that the value before be false and the value be true
        //
        // For != type propositions
        // ========================
        // If all the values satisfy the proposition then true
        // If all the values do not satisfy the proposition then false
        // Otherwise, require that either the value before be true or the value be false
        let compute_constraint_for_prop_eq var comparison_function range time = 
            if not (List.exists comparison_function range) then
                z.MkFalse()
            elif range.Length = 1 then
                z.MkTrue()
            else
                let index_of_value_satisfying_proposition = List.findIndex comparison_function range
                let z3_var = 
                    if (index_of_value_satisfying_proposition = (range.Length-1)) then
                        z.MkTrue()
                    else
                        let value_satisfying_prop = List.nth range index_of_value_satisfying_proposition
                        let z3_var_name = BioCheckPlusZ3.get_z3_bool_var_at_time_in_val var time value_satisfying_prop
                        BioCheckPlusZ3.make_z3_bool_var z3_var_name z
                let neg_z3_var_before = 
                    if (index_of_value_satisfying_proposition = 0) then 
                        z.MkTrue()
                    else
                        let value_before = List.nth range (index_of_value_satisfying_proposition-1)
                        let z3_var_before = BioCheckPlusZ3.get_z3_bool_var_at_time_in_val var time value_before
                        z.MkNot(BioCheckPlusZ3.make_z3_bool_var z3_var_before z)
                z.MkAnd(z3_var,neg_z3_var_before)

(*        let compute_constraint_for_prop_neq var comparison_function range time = 
            let negation_of_comparison_function number = not (comparison_function number)
            if not (List.exists comparison_function range) then
                z.MkTrue()
            elif range.Length = 1 then
                z.MkFalse() 
            else
                let index_of_value_not_satisfying_proposition = List.findIndex negation_of_comparison_function range
                let not_z3_var = 
                    if (index_of_value_not_satisfying_proposition = (range.Length-1)) then
                        z.MkFalse()
                    else
                        let value_not_satisfying_prop = List.nth range index_of_value_not_satisfying_proposition
                        let z3_var_name = BioCheckPlusZ3.get_z3_bool_var_at_time_in_val var time value_not_satisfying_prop
                        z.MkNot(BioCheckPlusZ3.make_z3_bool_var z3_var_name z)
                let z3_var_before = 
                    if (index_of_value_not_satisfying_proposition = 0) then
                        z.MkFalse()
                    else
                        let value_before= List.nth range (index_of_value_not_satisfying_proposition-1)
                        let z3_var_before = BioCheckPlusZ3.get_z3_bool_var_at_time_in_val var time value_before
                        BioCheckPlusZ3.make_z3_bool_var z3_var_before z
                z.MkOr(not_z3_var, z3_var_before)
                *)

        let compute_constraint_for_prop_gt var comparison_function range time =
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

        let compute_constraint_for_prop_lt var comparison_function range time =
            let negation_of_comparison_function number = not (comparison_function number)                
            if not (List.exists negation_of_comparison_function range) 
            then
                // All values in the range satisfy the proposition
                z.MkTrue()
            else
                let first_value_not_satisfying_proposition = List.findIndex negation_of_comparison_function range                    
                if (first_value_not_satisfying_proposition = 0)
                then
                    // All values do not satisfy proposition
                    z.MkFalse()
                else
                    // Find the last value that satisfies the proposition
                    // If the appropriate z3 variable is true then one of the values satisfying
                    // the proposition is true
                    let last_value_satisfying_proposition = List.nth range (first_value_not_satisfying_proposition-1)
                    let z3_var_name = BioCheckPlusZ3.get_z3_bool_var_at_time_in_val var time last_value_satisfying_proposition
                    BioCheckPlusZ3.make_z3_bool_var z3_var_name z

        // Prepare the necessary z3 constraints for the operands
        let l_op_encode =
            match ltl_formula with 
            | Until (_, op, _)
            | Wuntil (_, op, _)
            | Release (_, op, _)
            | Upto (_, op, _)
            | And (_, op, _) 
            | Or (_, op, _) 
            | Implies (_, op, _) 
            | Not (_, op)
            | Next (_, op)
            | Always (_, op)
            | Eventually (_, op) ->
                encode_truth_of_formula op
            | _ ->
                z.MkTrue()

        let r_op_encode =
            match ltl_formula with 
            | Until (_, _, op)
            | Wuntil (_, _, op)
            | Release (_, _, op)
            | Upto (_, _, op)
            | And (_, _, op) 
            | Or (_, _, op) 
            | Implies (_, _, op) ->
                encode_truth_of_formula op 
            // Unary operators, propositions, true, false
            | _ ->
                z.MkTrue()

        let variable_range = 
            match ltl_formula with
            // Propositions
            | PropEq (_, var, _)
            | PropNeq (_, var, _)
            | PropGt (_, var , _)
            | PropGtEq (_, var, _)
            | PropLt (_, var, _)
            | PropLtEq (_, var, _) ->
                Map.find var.var range
            | _ ->
                 []

        let comparison_function =
            match ltl_formula with
            | PropNeq (_, _, value) (* -> (fun number -> number <> value) *)
            | PropEq (_, _, value) -> (fun number -> number = value) 
            | PropGt (_, _ , value) -> (fun number -> number > value)
            | PropGtEq (_, _,  value) -> (fun number -> number >= value)
            | PropLt (_, _, value) -> (fun number -> number < value)
            | PropLtEq (_, _, value) -> (fun number -> number <= value)
            | _ -> (fun number -> true)

        // Build the actual constraint corresponding to the formula at previous and current
        let z3_constraint = 
            match ltl_formula with 
            | Until (location, _, _)
            | Wuntil (location, _, _)
            | Release (location, _, _) 
            | Upto (location, _, _)
            | Next (location, _) 
            | Always (location, _) 
            | Eventually (location, _) ->
                create_z3_bool_var location step z
            | And (_, _, _) ->
                z.MkAnd(l_op_encode,r_op_encode)
            | Or (_, _, _) ->
                z.MkOr(l_op_encode,r_op_encode)
            | Implies (_, _, _) ->
                z.MkImplies(l_op_encode,r_op_encode)
            | Not (_, _) ->
                z.MkNot(l_op_encode)
            | PropEq (_, var, _) ->
                compute_constraint_for_prop_eq var comparison_function variable_range step
            | PropNeq (_, var, _) ->
                z.MkNot( compute_constraint_for_prop_eq var comparison_function variable_range step)
            | PropGt (_ , var , _) 
            | PropGtEq (_ , var , _) ->
                compute_constraint_for_prop_gt var comparison_function variable_range step
            | PropLt (_ , var , _) 
            | PropLtEq (_ , var , _) ->
                compute_constraint_for_prop_lt var comparison_function variable_range step
            | False ->
                z.MkFalse()
            | _ ->
                z.MkTrue()

        let z3_hidden_subformula_constraint =
            match ltl_formula with
            | Upto (location, _, _) ->
                create_z3_bool_var (2::location) step z
            | _ ->
                z.MkTrue()

        match ltl_formula with
        | Upto (location, _, _) ->
            return_map := Map.add (2::location) z3_hidden_subformula_constraint !return_map
            ignore(return_map := Map.add location z3_constraint !return_map)
        | Until (location, _, _)
        | Wuntil (location, _, _)
        | Release (location, _, _)
        | Next (location, _) 
        | Always (location, _) 
        | Eventually (location, _) 
        | And (location, _, _) 
        | Or (location, _, _) 
        | Implies (location, _, _) 
        | Not (location, _) 
        | PropEq (location, _, _)
        | PropNeq (location, _, _)
        | PropGt (location, _, _) 
        | PropGtEq (location, _, _) 
        | PropLt (location , _, _) 
        | PropLtEq (location , _, _) ->      
            ignore(return_map := Map.add location z3_constraint !return_map)
        | _ ->
            ()

        z3_constraint

    ignore(encode_truth_of_formula ltl_formula)
    !return_map

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
let encode_formula_transition_from_to (ltl_formula : LTLFormulaType) (network : QN) (previous_map : FormulaConstraint) (current_map : FormulaConstraint) (step_prev : int) (step_curr : int) (z : Context) additional_application_constraint = 
    let rec assert_transition_of_formula ltl_formula =

        // Call recursively
        match ltl_formula with 
        | Until (_, l, r)
        | Wuntil (_, l, r)
        | Release (_, l, r)
        | Upto (_, l, r)
        | And (_, l, r) 
        | Or (_, l, r) 
        | Implies (_, l, r) -> 
            assert_transition_of_formula l
            assert_transition_of_formula r
            ()
        | Not (_, op)
        | Next (_, op)
        | Always (_, op)
        | Eventually (_, op) ->
            assert_transition_of_formula op
            ()
        | _ -> 
            ()

        let truez3 = z.MkTrue()
        let falsez3 = z.MkFalse()
        // Call recursively the encoding of the formula
        // Get the encoding of the formula for current and next
        // Compute their negations and return all four
        let constraint_for_formula_and_neg_prev_curr operand =
            match operand with 
            | Until (location, _, _)
            | Wuntil (location, _, _)
            | Release (location, _, _)
            | Upto (location, _, _)
            | And (location, _, _) 
            | Or (location, _, _) 
            | Implies (location, _, _) 
            | Not (location, _)
            | Next (location, _)
            | Always (location, _)
            | Eventually (location, _) 
            | PropEq (location, _, _)
            | PropNeq (location, _, _)
            | PropGt (location, _ , _)
            | PropGtEq (location, _, _)
            | PropLt (location, _, _)
            | PropLtEq (location, _, _) ->
                let op_at_prev = Map.find location previous_map
                let op_at_curr = Map.find location current_map
                let not_op_at_prev = z.MkNot(op_at_prev) // This does not work for propositions!
                let not_op_at_curr = z.MkNot(op_at_curr) // This does not work for propositions!
                (op_at_prev,not_op_at_prev,op_at_curr,not_op_at_curr)
            | False ->
                (falsez3, truez3, falsez3, truez3)
            | _ ->
                (truez3, falsez3, truez3, falsez3)

        // Collect the necessary z3 constraints for the operands
        // Prepare a constraint for:
        // 1. The formula is true at prev
        // 2. The formula is false at prev
        // 3. The formula is true at current
        // 4. The formula is false at current
        let (l_at_prev,not_l_at_prev,l_at_current, not_l_at_current) =
            match ltl_formula with 
            | Until (_, op, _)
            | Wuntil (_, op, _)
            | Release (_, op, _)
            | Upto (_, op, _)
            | And (_, op, _) 
            | Or (_, op, _) 
            | Implies (_, op, _) 
            | Not (_, op)
            | Next (_, op)
            | Always (_, op)
            | Eventually (_, op) ->
                constraint_for_formula_and_neg_prev_curr op
            // Propositions, true, false
            | _ ->
                (truez3, truez3, truez3, truez3)

        let (r_at_prev,not_r_at_prev,r_at_current,not_r_at_current) =
            match ltl_formula with 
            | Until (_, _, op)
            | Wuntil (_, _, op)
            | Release (_, _, op)
            | Upto (_, _, op)
            | And (_, _, op) 
            | Or (_, _, op) 
            | Implies (_, _, op) ->
                constraint_for_formula_and_neg_prev_curr op
            // Unary operators, propositions, true, false
            | _ ->
                (truez3, truez3, truez3, truez3)

        let constraint_for_hidden_subformula_and_neg_prev_curr operand =
            match operand with 
            | Until (location, _, _)
            | Wuntil (location, _, _)
            | Release (location, _, _)
            | Upto (location, _, _)
            | And (location, _, _) 
            | Or (location, _, _) 
            | Implies (location, _, _) 
            | Not (location, _)
            | Next (location, _)
            | Always (location, _)
            | Eventually (location, _) 
            | PropEq (location, _, _)
            | PropNeq (location, _, _)
            | PropGt (location, _ , _)
            | PropGtEq (location, _, _)
            | PropLt (location, _, _)
            | PropLtEq (location, _, _) ->
                let op_at_prev = Map.find location previous_map
                let op_at_curr = Map.find location current_map
                let not_op_at_prev = z.MkNot(op_at_prev) // This does not work for propositions!
                let not_op_at_curr = z.MkNot(op_at_curr) // This does not work for propositions!
                (op_at_prev,not_op_at_prev,op_at_curr,not_op_at_curr)
            | False ->
                (falsez3, truez3, falsez3, truez3)
            | _ ->
                (truez3, falsez3, truez3, falsez3)

        let (hidden_sub_at_prev, not_hidden_sub_at_prev, hidden_sub_at_current, not_hidden_sub_at_current) =
            match ltl_formula with
            | Upto (location, _, _) ->
                let op_at_prev = Map.find (2::location) previous_map
                let op_at_curr = Map.find (2::location) current_map
                let not_op_at_prev = z.MkNot(op_at_prev)
                let not_op_at_curr = z.MkNot(op_at_curr)
                (op_at_prev,  not_op_at_prev, op_at_curr, not_op_at_curr)
            | _ ->
                (truez3, truez3, truez3, truez3)

        let (temporal_var_at_prev, not_temporal_var_at_prev, temporal_var_at_current, not_temporal_var_at_current) =
            match ltl_formula with            
            // Temporal operators
            | Until (location, _, _)
            | Wuntil (location, _, _)
            | Release (location, _, _)
            | Upto (location, _, _)
            | Next (location, _)
            | Always (location, _)
            | Eventually (location, _) ->
                constraint_for_formula_and_neg_prev_curr ltl_formula
            // Non temporal operators
            | _ ->
                let trueZ3 = z.MkTrue()
                (trueZ3, trueZ3, trueZ3, trueZ3)

        // Build the actual constraint corresponding to the formula at previous and current
        let constraint_for_formula = 
            match ltl_formula with 
            | Until (location, left_op, right_op) 
            | Wuntil (location, left_op, right_op) ->
                let true_of_until = 
                    z.MkImplies(temporal_var_at_prev, 
                                 z.MkOr(r_at_prev,
                                        z.MkAnd(l_at_prev, temporal_var_at_current)))
                let false_of_until = 
                    z.MkImplies(not_temporal_var_at_prev, 
                                z.MkAnd(not_r_at_prev, 
                                        z.MkOr(not_l_at_prev,not_temporal_var_at_current)))
                z.MkAnd(true_of_until,false_of_until)

            | Release (location, left_op, right_op) ->
                let true_of_release = 
                    z.MkImplies(temporal_var_at_prev, 
                                z.MkOr(z.MkAnd(l_at_prev, r_at_prev), 
                                       z.MkAnd(r_at_prev, temporal_var_at_current)))
                let false_of_release =
                    z.MkImplies(not_temporal_var_at_prev,
                                z.MkAnd(z.MkOr(not_l_at_prev, not_r_at_prev),
                                        z.MkOr(not_r_at_prev, not_temporal_var_at_current)))
                z.MkAnd(true_of_release,false_of_release)
            | Upto (location, left_op, right_op) ->
                let true_of_hidden = 
                    z.MkImplies(hidden_sub_at_prev,
                                z.MkOr(r_at_prev,
                                       z.MkAnd(l_at_prev,hidden_sub_at_current)))
                let false_of_hidden = 
                    z.MkImplies(not_hidden_sub_at_prev,
                                z.MkAnd(not_r_at_prev,
                                        z.MkOr(not_l_at_prev, not_hidden_sub_at_current)))
                let true_of_upto =
                    z.MkImplies(temporal_var_at_prev, 
                                z.MkAnd(l_at_prev, hidden_sub_at_current))
                let false_of_upto = 
                    z.MkImplies(not_temporal_var_at_prev,
                                z.MkOr(not_l_at_prev, not_hidden_sub_at_current))
                z.MkAnd(z.MkAnd(true_of_hidden,false_of_hidden),z.MkAnd(true_of_upto,false_of_upto))
            | Next (location, op) ->
                let true_of_next = 
                    z.MkImplies(temporal_var_at_prev, l_at_current)
                let false_of_next = 
                    z.MkImplies(not_temporal_var_at_prev, not_l_at_current)
                z.MkAnd(true_of_next,false_of_next)

            | Always (location, op) ->
                let true_of_always = 
                    z.MkImplies(temporal_var_at_prev,
                                z.MkAnd(l_at_prev,temporal_var_at_current))
                let false_of_always = 
                    z.MkImplies(not_temporal_var_at_prev,
                                z.MkOr(not_l_at_prev, not_temporal_var_at_current))
                z.MkAnd(true_of_always,false_of_always)

            | Eventually (location, op) ->
                let true_of_eventually = 
                    z.MkImplies(temporal_var_at_prev,
                                z.MkOr(l_at_prev,temporal_var_at_current))
                let false_of_eventually = 
                    z.MkImplies(not_temporal_var_at_prev,
                                z.MkAnd(not_l_at_prev,not_temporal_var_at_current))
                z.MkAnd(true_of_eventually,false_of_eventually)
            | _ ->
                z.MkTrue ()
        match ltl_formula with 
        | Until (location, _, _)
        | Wuntil (location, _, _)
        | Release (location, _, _)
        | Upto (location, _, _)
        | Next (location, _)
        | Always (location, _)
        | Eventually (location, _) ->
            let implication = z.MkImplies(additional_application_constraint,constraint_for_formula)
            z.AssertCnstr(implication)
        // Non temporal operators
        | _ ->
            ()

    assert_transition_of_formula ltl_formula


let encode_formula_transition_in_loop (ltl_formula : LTLFormulaType) (network : QN) (last_map : FormulaConstraint) (current_map : FormulaConstraint) (last_step : int) (current_step : int) (z : Context) = 
    let z3_constraint_for_loop_at_time = BioCheckPlusZ3.constraint_for_loop_at_time current_step last_step z
    ignore(encode_formula_transition_from_to ltl_formula network last_map current_map last_step  current_step z z3_constraint_for_loop_at_time)


let encode_formula_fairness_in_loop (ltl_formula :LTLFormulaType) (network : QN)  (range : VariableRange) (current_time : int) (last_time : int) (z : Context) =
    let number1 = 1
    let number2 = 0
    let error = number1 / number2
    // Essentially for each temporal formula, say that 
    ()

let create_list_of_maps_of_formula_constraints (ltl_formula : LTLFormulaType) (network : QN) (z: Context) (ranges : VariableRange list) =
    let time = ref 0
    let list_of_maps = ref []
    for range in ranges do
        let map = produce_constraints_for_truth_of_formula_at_time ltl_formula network range !time z
        list_of_maps := !list_of_maps @ (map::[])

        incr time

    !list_of_maps

let encode_formula_transitions_over_path (ltl_formula : LTLFormulaType) (network :QN) (z : Context) (formula_constraints : FormulaConstraintList) =
    let time = ref 1
    let previous_constraint = ref formula_constraints.Head
    let remaining_formula_constraints = formula_constraints.Tail

    for current_constraint in remaining_formula_constraints do
        encode_formula_transition_from_to ltl_formula network !previous_constraint current_constraint (!time-1) !time z (z.MkTrue())

        incr time
        previous_constraint := current_constraint

    ()

let encode_formula_transitions_in_loop_closure (ltl_formula : LTLFormulaType) (network : QN) (z : Context) (formula_constraints : FormulaConstraintList) =
    let last_time = formula_constraints.Length - 1
    let last_map = List.head (List.rev formula_constraints)

    let time = ref 0
    for current_map in formula_constraints do
        encode_formula_transition_in_loop ltl_formula network last_map current_map last_time !time z

        incr time
    ()

let constraint_of_hidden_subformula (ltl_formula : LTLFormulaType) (map : FormulaConstraint) (z : Context) =
    match ltl_formula with
    | Upto (location, _, _) ->
        Map.find (2::location) map
    | _ ->
        z.MkFalse()

let constraint_of_formula (ltl_formula : LTLFormulaType) (map : FormulaConstraint) (z : Context)=
    match ltl_formula with
    | Until (location, _, _)
    | Wuntil (location, _, _)
    | Release (location, _, _)
    | Upto (location, _, _)
    | And (location, _, _) 
    | Or (location, _, _) 
    | Not (location, _)
    | Next (location, _)
    | Always (location, _)
    | Eventually (location, _)
    | PropEq (location, _, _)
    | PropNeq (location, _, _) 
    | PropGt (location, _ , _)
    | PropGtEq (location, _, _)
    | PropLt (location, _, _)
    | PropLtEq (location, _, _) ->
        Map.find location map
    | False ->
        z.MkFalse()
    | _ ->
        z.MkTrue()

//     EncodingForFormula.assert_top_most_formula ltl_formula ctx list_of_maps.Head
let assert_top_most_formula (ltl_formula : LTLFormulaType) (z : Context) (map : FormulaConstraint) =
    let top_most_formula = constraint_of_formula ltl_formula map z
    z.AssertCnstr(top_most_formula)

// Go recursively over the formula structure
// For each temporal formula that requires fairness (until, release, always, eventually)
// assert the disjunction that the fairness holds at some point in the loop.
// This is a disjunction of all possible locations saying for each location:
// this is a possible loop location and it is fair.
// In symbols: \/_{time=0}^last_time (inside_loop /\ fair)
// The inside loop predicate is: if time=last_time then it is true
//                               if time<last_time then it is l_time
let encode_formula_loop_fairness (ltl_formula : LTLFormulaType) (network : QN) (z : Context) (formula_constraints : FormulaConstraintList) =
    let last_time = formula_constraints.Length - 1

    let compute_loop_possible_at_time (time : int) (last_time : int) =
        if (time = last_time)
        then
            z.MkTrue()
        else
            let name_of_loop_var = BioCheckPlusZ3.get_z3_bool_var_loop_at_time time
            BioCheckPlusZ3.make_z3_bool_var name_of_loop_var z
    
    let encode_fairness_for_formula (ltl_formula : LTLFormulaType) = 
        let time = ref 0
        let list_of_disjuncts = ref []

        for formula_constraint in formula_constraints do
            let constraint_loop_possible_at_time = compute_loop_possible_at_time !time last_time
            
            let constraint_for_formula = constraint_of_formula ltl_formula formula_constraint z

            let constraint_for_hidden_subformula = 
                match ltl_formula with
                | Upto (loc, _, _) ->
                    constraint_of_hidden_subformula ltl_formula formula_constraint z
                | _ ->
                    z.MkFalse()

            let constraint_for_formula_fairness = 
                match ltl_formula with
                | Until (_, _, op) 
                | Eventually (_, op) ->
                    let constraint_for_op = constraint_of_formula op formula_constraint z
                    let not_constraint_for_formula = z.MkNot (constraint_for_formula)
                    z.MkOr(not_constraint_for_formula, constraint_for_op)
                | Release (_, _, op) 
                | Always (_, op) ->
                    let constraint_for_op = constraint_of_formula op formula_constraint z
                    let not_constraint_for_op = z.MkNot(constraint_for_op)
                    z.MkOr(constraint_for_formula,not_constraint_for_op)
                | Wuntil (_, op1, op2) ->
                    let constraint_for_op1 = constraint_of_formula op1 formula_constraint z
                    let not_constraint_for_op1 = z.MkNot(constraint_for_op1)
                    let constraint_for_op2 = constraint_of_formula op2 formula_constraint z
                    let not_constraint_for_op2 = z.MkNot(constraint_for_op2)
                    z.MkOr(constraint_for_formula,z.MkAnd(not_constraint_for_op1,not_constraint_for_op2))
                | Upto (_, _, op) ->
                    let constraint_for_op = constraint_of_formula op formula_constraint z
                    let not_constraint_for_hidden_sub = z.MkNot(constraint_for_hidden_subformula)
                    z.MkOr(not_constraint_for_hidden_sub,constraint_for_op)
                | _ ->
                    z.MkTrue()

            let disjunct = z.MkAnd(constraint_for_formula_fairness,constraint_loop_possible_at_time)
            list_of_disjuncts := disjunct::(!list_of_disjuncts)
            
            incr time

        let array_of_disjuncts = List.toArray !list_of_disjuncts
        let fairness_constraint = z.MkOr(array_of_disjuncts)
        ignore(z.AssertCnstr(fairness_constraint))

    let rec recursive_encode_formula_fairness ltl_formula = 

        match ltl_formula with 
        | Until (location, _, _) 
        | Upto (location, _, _)
        | Release (location,_ ,_)
        | Always (location, _)
        | Eventually (location, _) ->
            ignore(encode_fairness_for_formula ltl_formula)
        | _ ->
            ()

        match ltl_formula with 
        | Until (_ , l_op, r_op)
        | Upto (_, l_op, r_op)
        | Release (_, l_op, r_op)
        | And (_, l_op, r_op)
        | Or (_, l_op, r_op) ->
            recursive_encode_formula_fairness l_op
            recursive_encode_formula_fairness r_op
        | Not (_, op)
        | Next (_, op)
        | Always (_, op)
        | Eventually (_, op) ->
            recursive_encode_formula_fairness op
        |_ ->
            ()
        ()

    recursive_encode_formula_fairness ltl_formula

