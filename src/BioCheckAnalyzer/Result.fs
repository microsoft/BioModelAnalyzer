// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
module Result


            // time , var->bound
type history = (int * QN.interval) list 

/// Initial stability check (yes or no). 
type stability_result =
    | SRStabilizing of    history
    | SRNotStabilizing of history
    
/// If SRNotStabilizing, then we have a CEx. 
type cex_result = 
    | CExBifurcation of Map<string, int> * Map<string, int>
    | CExCycle of Map<string, int>
    | CExFixpoint of Map<string, int>
    | CExEndComponent of Map<string, int>
    | CExUnknown
    override this.ToString() =
        let string_of_fixpoint fixpoint =
            List.map (fun (var,value) -> sprintf "%s=%d" var value) (Map.toList fixpoint)
        match this with
            | CExBifurcation(fix1,fix2) -> 
                "Bifurcates with fix1:" + (String.concat "," (string_of_fixpoint fix1)) + " fix2:" + (String.concat "," (string_of_fixpoint fix2))
            | CExCycle(cyc) ->
                "Cycles:" + (String.concat "," (string_of_fixpoint cyc))
            | CExFixpoint(fix) ->
                "Fixpoint:" + (String.concat "," (string_of_fixpoint fix))
            | CExEndComponent(endComp) ->
                "End component:" + (String.concat "," (string_of_fixpoint endComp) )
            | CExUnknown -> 
                "Unknown"


let stabilizes r = match r with SRStabilizing _ -> true | _ -> false
let bifurcates r = match r with CExBifurcation _ -> true | _ -> false
let cycles r = match r with CExCycle _ -> true | _ -> false
let fixpoints r = match r with CExFixpoint _ -> true | _ -> false


