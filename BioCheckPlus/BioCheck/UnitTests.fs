(* Copyright (c) Microsoft Corporation. All rights reserved. *)
module UnitTests

open TestNetworks

/// Unit tests!

let stabilizes range net =
    match Stabilize.stabilization_proving_proc net range with
    | Result.Stabilizing(fixpoint) -> true
    | _ -> false

let has_fixpoint_z3 range net =
    match BioCheckZ3.find_fixpoint net range with
    | Some(model) -> true
    | None -> false

let bifurcates_z3 range net =
    match BioCheckZ3.find_bifurcation net range with
    | Some(bifurcation) -> true
    | None -> false

let loops_z3 range (net : QN.node list) =
    let num_vars = List.length net
    match BioCheckZ3.find_cycle net range -1 range with
    | Some(cycle) -> true
    | None -> false

let inline test_network net does_stabilize has_fixpoint does_bifurcate does_loop range =
    Test.register_test does_stabilize (fun () -> net |> stabilizes range)
    Test.register_test has_fixpoint (fun () -> net |> has_fixpoint_z3 range)
    Test.register_test does_bifurcate (fun () -> net |> bifurcates_z3 range)
    Test.register_test does_loop (fun () -> net |> loops_z3 range)


let register_tests () =
    let N = 2

    // A const singleton does stabilize, does have a fixpoint, does not bifurcate
    // and does not have a non-trivial cycle.
    let q = mk_const_singleton 0 
    let range = List.fold (fun range (v:QN.node) -> Map.add v.var (0,N) range) Map.empty q 
    test_network q true true false false range

    // A singleton flipflop does not stabilize, doesn't,
    // doesn't bifurcate and does have a non-trivial cycle.
    let q = mk_flipflop_singleton()
    let range = List.fold (fun range (v:QN.node) -> Map.add v.var (0,N) range) Map.empty q   
    test_network q false false false true range

    // A pair of flipflops doesn't stabilize, does have a fixpoint,
    // does bifurcate (x=0,y=1; x=1,y=0) and does have a non-trivial cycle.
    let q = mk_flipflop_pair()
    let range = List.fold (fun range (v:QN.node) -> Map.add v.var (0,N) range) Map.empty q   
    test_network q false true true true range

    // A singleton with an id target function doesn't stabilize, does have a fixpoint,
    // does bifurcate and doesn't have a non-trivial cycle.
    let q = mk_id_singleton()
    let range = List.fold (fun range (v:QN.node) -> Map.add v.var (0,N) range) Map.empty q   
    test_network q false true true false range
