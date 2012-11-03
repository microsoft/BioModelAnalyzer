(* Copyright (c) Microsoft Corporation. All rights reserved. *)
module TestNetworks

/// Simple networks used as test cases.

/// Create a network consisting of a single node with a constant target function.
let mk_const_singleton x =
    let node_target = Expr.Const x
    let node : QN.node = { var = QN.mk_var(); f = node_target; inputs = []; min = 0; max = 1; name = "const_node" }
    [ node ]


/// Create a network consisting of a single node that oscillates with period=2
let mk_flipflop_singleton () =
    let node_var = QN.mk_var()
    let node_target = Expr.Max(Expr.Const 0, Expr.Minus(Expr.Const 1, Expr.Var node_var))
    let node : QN.node = { var = node_var; f = node_target; inputs = [ node_var ]; min = 0; max = 1; name = "flipflop_node" }
    [ node ]


let mk_flipflop_pair () =
    let node1_var = QN.mk_var()
    let node2_var = QN.mk_var()
    let node1_target = Expr.Max(Expr.Const 0, Expr.Minus(Expr.Const 1, Expr.Var node2_var))
    let node2_target = Expr.Max(Expr.Const 0, Expr.Minus(Expr.Const 1, Expr.Var node1_var))
    let node1 : QN.node = { var = node1_var; f = node1_target; inputs = [ node2_var ]; min = 0; max = 1; name = "flipflop_node1" }
    let node2 : QN.node = { var = node2_var; f = node2_target; inputs = [ node1_var ]; min = 0; max = 1; name = "flipflop_node2" }
    [ node1; node2 ]


/// Create a network consisting of a single node that has N fixpoints.
let mk_id_singleton () =
    let node_var = QN.mk_var()
    // The target function is the id function!
    let node_target = Expr.Var node_var
    let node : QN.node = { var = node_var; f = node_target; inputs = [ node_var ]; min = 0; max = 1; name = "id_node" }
    [ node ]




// F# version of matt/BioCheck/test/check_result.py
let map_of_file f =
    let lines = System.IO.File.ReadLines f
    Seq.fold
        (fun map (l:string) ->
            let aa = l.Split('=')
            if (Array.length aa) = 2 then Map.add (aa.[0].Trim()) (aa.[1].Trim()) map
            else map)
        Map.empty
        lines

let compare (d1:Map<string,string>) (d2:Map<string,string>) =
    let total = ref 0
    let agree = ref 0
    Map.iter
        (fun k1 v1 ->
            if Map.containsKey k1 d2 then
                total := !total + 1
                let v2 = Map.find k1 d2
                if not (v1 = v2) then
                    printfn "%s => %s, %s" k1 v1 v2
                else agree := !agree + 1
        )
        d1
    printfn "Agree on %d/%d" !agree !total


let j = @"C:\\src\matt\BioCheck\test\java-skin1d-9"
let f = @"C:\\src\matt\BioCheck\test\f-new-skin1d-9"

let test _ =
    let f_map = map_of_file f
    let j_map = map_of_file j
    ()

