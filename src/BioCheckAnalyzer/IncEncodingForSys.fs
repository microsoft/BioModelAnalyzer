// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
module IncEncodingForSys

open Microsoft.Z3

let incZ3forSysOneStep network initBound (fixedRange : Map<QN.var, int list>) increSteps (z : Context) (s : Solver) =
    for node in network do
        // assert one fixed range now    
        BioCheckZ3.assert_bound node ((List.min(Map.find node.var fixedRange)) , (List.max(Map.find node.var fixedRange))) increSteps z s
        // assert one trans for system
        let nodeinputlist =
            List.concat
                [ for var in node.inputs do
                    yield (List.filter (fun (x:QN.node) -> x.var = var) network) ]
        let nodelist = node :: nodeinputlist
        let varnamenode = BioCheckZ3.build_var_name_map nodelist
        stepZ3rangelist.assert_target_function network node varnamenode initBound (increSteps-1) increSteps z s
