(* Copyright (c) Microsoft Corporation. All rights reserved. *)
module QN

/// A Qualtitative Network.

// Variables are just referenced by their index.
type var = int

// Return a fresh variable name.
let mutable num_vars = 0
let mk_var _ =
    let n = num_vars
    num_vars <- num_vars + 1
    n

// what is this used for?
let mk_var_unsafe i = 
    i

/// A node in a QN network.
type node =
    {
        var : var;          // The variable associated with this node's output.
        f : Expr.expr;      // The target function for this node.
        inputs : var list;  // The variables this node depends on.
        name : string;
        min : int;
        max : int;
    }

let str_of_node (n:node) = 
    let ii = String.concat "," (List.map (fun v -> (string)v) n.inputs)                    
    let f = Expr.str_of_expr n.f
    sprintf "{var=%d; inputs={%s}; f=(%s)}" n.var ii f

/// 
type range = Map<var,int*int>
let str_of_range range = String.concat "," (Map.fold (fun st v (lo,hi) -> (sprintf "%d=(%d,%d)" v lo hi)::st) [] range)    

/// Well-formed QN network 
/// Raises exn if qn isn't wf. 
let qn_wf qn = 
    // all ids are unique
    let uniq_ids = List.fold
                        (fun uniq_ids (n:node) -> 
                            let n_id = n.var 
                            match Map.tryFind n_id uniq_ids with 
                            | None -> Map.add n_id n uniq_ids 
                            | Some _ -> failwith ("Two entries for " + (string)n_id))
                        Map.empty 
                        qn 
    // Transfer functions only mention valid ids 
    List.iter 
        (fun (n:node) -> 
            let vv_in_f = Expr.fv n.f
            let vv_valid = Set.forall (fun v -> Map.containsKey v uniq_ids) vv_in_f 
            if (not vv_valid ) then 
                let bad_vv = Set.fold (fun st v -> (string)v + st) "" vv_in_f
                failwith ("A T input in not a variable: " + bad_vv))
        qn 
  
    
        
//let print_qn network = 
//    List.iter
//        (fun (n:node) -> 
//            printf "name:%s var:%d " n.name n.var
//            printf "inputs: " 
//            List.iter (fun i -> printf "%d, " i) n.inputs
//            printf "T: "
//            Expr.print_expr n.f
//            printfn "")
//        network



