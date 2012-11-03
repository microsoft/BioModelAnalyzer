(* Copyright (c) Microsoft Corporation. All rights reserved. *)
module Result

// SI: the maps should be QN.var->int, not string. 
type result = Stabilizing of Map<string, int>
              | Bifurcation of Map<string, int> * Map<string, int>
              | Cycle of Map<string, int>
              | Fixpoint of Map<string, int>
              | Unknown
              override this.ToString() =
                let string_of_fixpoint fixpoint =
                    List.map (fun (var,value) -> sprintf "%s=%d" var value) (Map.toList fixpoint)
                match this with
                  | Stabilizing(fix) ->
                        "Stabilizing:" + (String.concat "," (string_of_fixpoint fix))
                  | Bifurcation(fix1,fix2) ->
                        "Bifurcates with fix1:" + (String.concat "," (string_of_fixpoint fix1)) + " fix2:" + (String.concat "," (string_of_fixpoint fix2))
                  | Cycle(cyc) ->
                        "Cycles:" + (String.concat "," (string_of_fixpoint cyc))
                  | Fixpoint(fix) ->
                        "Fixpoint:" + (String.concat "," (string_of_fixpoint fix))
                  | Unknown ->
                        "Unknown"

let iter_fix fix = Map.iter (fun v i -> printfn "%s=%d" v i) fix
let str_of_fix fix = String.concat "," (Map.fold (fun st id v -> (sprintf "%s=%d" id v)::st) [] fix)

let print_result r =    
    match r with
    | Stabilizing(fix) ->
        printfn "Stabilizing:"; iter_fix fix
    | Bifurcation(fix1,fix2) ->
        printfn "Bifurcates with fix1:"; iter_fix fix1; printfn " fix2:"; iter_fix fix2
    | Cycle(cyc) ->
        printfn "Cycles:"; iter_fix cyc
    | Fixpoint(fix) ->
        printfn "Fixpoint:"; iter_fix fix
    | Unknown ->
        printfn "Unknown"

let str_of_result r =    
    match r with
    | Stabilizing(fix) ->        
        sprintf "Stabilizing: {%s}" (str_of_fix fix)
    | Bifurcation(fix1,fix2) ->
        sprintf "Bifurcates with fix1: {%s}, fix2:{%s}" (str_of_fix fix1) (str_of_fix fix2)
    | Cycle(cyc) ->
        sprintf "Cycles: {%s}" (str_of_fix cyc)
    | Fixpoint(fix) ->
        sprintf "Fixpoint: {%s}" (str_of_fix fix)
    | Unknown ->
        "Unknown"

type result_steps = ResultStepping of Map<QN.var,int*int>
                  | ResultLastStep of result 
