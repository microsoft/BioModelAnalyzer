// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
module QN

/// A Qualtitative Network.

// Variables are just referenced by their index.
type var = int
type nature = Act | Inh
type number = int
type pos = int
type cell = string


// Return a fresh variable name.
let mutable num_vars = 0
let mk_var _ =
    let n = num_vars
    num_vars <- num_vars + 1
    n

let mk_var_unsafe i =
    i

/// A node in a QN network.
type node =
    {
        // Basic graph data
        var : var;          // The variable associated with this node's output.
        f : Expr.expr;      // The target function for this node.
        inputs : var list;  // The variables this node depends on.
        range : int * int;  // [min..max]
        name : string;

        // Further connectivity data (used by Garvit's engine)
        nature : Map<var, nature>; // nature of each input, must have as many elements as inputs
        defaultF : bool; //whether the target function is the default one
        number : number; //the number that is same across the copies of the same protein across cells
        tags : (pos*cell) list //list of (tag position, tag name) each tag corresponds to a cell in the network
    }

let str_of_node (n:node) =
    let ii = String.concat "," (List.map (fun v -> (string)v) n.inputs)
    let (lo,hi) = n.range
    let f = Expr.str_of_expr n.f
    let tgs = String.concat "," (List.map (fun (p,c) -> (string)p+":"+c) n.tags)
    sprintf "{var=%d; range=[%d,%d]; name=%s; inputs={%s}; f=(%s); number=%d; tags=%s}" n.var lo hi n.name ii f n.number tgs

type qn = node list

///
type interval = Map<var, int*int>
type range = Map<var,int*int>

type env = Map<var,int>

let str_of_range network range =
    let names = Map.ofList (List.map (fun n -> n.var,n.name) network)
    String.concat ", " (Map.fold (fun st v (lo,hi) -> (sprintf "%s.%d:[%d,%d]" (Map.find v names) v lo hi)::st) [] range)

let str_of_env env =
    let l = Map.toList env
    String.concat ", " (List.map (fun (v,i) -> sprintf "(%d,%d)" v i) l)


// Next var, given current qn. 
// Safe way to ctor a node. 
let next_var qn = 
    let max_var = List.fold (fun max_so_far n -> max max_so_far n.var) 0 qn
    max_var + 1

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
    // Target functions only mention valid ids
    List.iter
        (fun (n:node) ->
            let vv_in_f = Expr.fv n.f
            let vv_valid = Set.forall (fun v -> Map.containsKey v uniq_ids) vv_in_f
            if (not vv_valid ) then
                let bad_vv = Set.fold (fun st v -> (string)v + st) "" vv_in_f
                failwith ("A T input in not a variable: " + bad_vv))
        qn

/// Check that env is complete wrt to qn
let env_complete_wrt_qn (qn:qn) (env:env) =
    if (List.length qn = env.Count) then
        // Each var \in qn is also defined in env
        let env_wrt_qn = List.forall (fun (n:node) -> Map.containsKey n.var env) qn
        // Each var \in env is also defined in qn
        let qn_wrt_env = Map.forall (fun v i -> List.exists (fun n -> n.var = v) qn) env
        // Each var \in env is is correctly bounded
        let env_bounded =
            Map.forall
                (fun v i ->
                            let n = List.find (fun n -> n.var=v) qn
                            let min,max = n.range
                            min <= i && i <= max)
                env
        env_wrt_qn && qn_wrt_env && env_bounded
    else false

// Used by Garvit ShrinkCut
let get_node_from_var (nv : var) (network : node list) = 
    List.find (fun n -> n.var = nv) network


/// Knockout the node [var] by replacing it's function with [c]
// Could be a more general function to replace a node's target function. Will then need to call expr parser here. 
let ko qn var c = 
    // Get the n and the rest
    let nn,qn_sub_n = List.partition (fun n -> n.var = var) qn
    let n' = 
        match nn with 
        // There should be only one nn
        | [n] -> { n with f = (Expr.Const(c)) } 
        | _ -> failwith("Non-existent or duplicate node " + (string)var + " in qn")
    // Add new n back to qn
    let qn' = n' :: qn_sub_n
    qn_wf qn'
    qn'


// Knockout edge. 
// Suppose src->dst \in qn, and x is new in qn. Then knockout src for dst (and only for dst) by 
// substituting (dst.f)[ x/src ] and adding an edge x->dst, where x.f=c .
let ko_edge qn src dst c = 
    let srcs,qn_sub_srcs = List.partition (fun n -> n.var = src) qn
    let dsts,qn_sub_srcs_dsts = List.partition (fun n -> n.var = dst) qn_sub_srcs
    let src,x,dst' = 
        match srcs,dsts with 
        // There should be only 1 src and 1 dst. 
        | [src],[dst] -> 
            let v = next_var qn 
            let x = { var= v; f= (Expr.Const(c)); inputs= []; range=(c,c); name= ("ko" + (string)(v));
                      // SI: no Garvit stuff right now.                
                      nature= Map.empty; defaultF=false; number= src.number; tags=[] } 
            let dst' = { dst with f= (Expr.subst_var dst.f src.var v); inputs= (v :: dst.inputs) }
            src,x,dst'

        | _ -> failwith("Non-existent or duplicate node " + (string)src + " or " + (string)dst + " in qn")
    let qn' = src :: x :: dst' :: qn_sub_srcs_dsts
    qn_wf qn'
    qn'
