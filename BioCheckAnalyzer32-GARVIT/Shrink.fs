module Shrink

/// Update bounds of var using curBounds of its inputs
let rec UpdateVarBounds (node: QN.node) (qn: QN.node list) (curBounds: Map<QN.var, int*int>) =
    if Oracle.AlwaysStrictlyIncreases node (fst curBounds.[node.var]) qn curBounds then
        UpdateVarBounds node qn 
            (Map.remove node.var curBounds |> Map.add node.var ((fst curBounds.[node.var])+1, snd curBounds.[node.var]))
    elif Oracle.AlwaysStrictlyDecreases node (snd curBounds.[node.var]) qn curBounds then
        UpdateVarBounds node qn
            (Map.remove node.var curBounds |> Map.add node.var (fst curBounds.[node.var], (snd curBounds.[node.var])-1))
    else
        curBounds


