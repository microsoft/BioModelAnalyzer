

module Oracle

let GetNewState (aNode:QN.node) aNodeState (qn: QN.node list) env =
    let ranges = Map.ofList [for node in qn -> (node.var, node.range)]

    env
    |> Map.ofList
    // add the value of var as aNodeState. replace in case var is an input to itself
    |> Map.add aNode.var aNodeState  
    
    |> Expr.eval_expr aNode.var ranges aNode.f 


// lt and gt are prefix versions of < and  >
let lt x y = x < y
let gt x y = x > y

let template (aNode: QN.node) aNodeState (qn: QN.node list) (bounds: Map<QN.var, int*int>) nature rel=
    let newState =
        // create the environment to check whether var increases
        // select the corner point according to 'nature'
        [ for input in aNode.inputs do
            if aNode.nature.[input] = nature then
                yield (aNode.var, snd bounds.[input])
            else
                yield (aNode.var, fst bounds.[input])]
        |> GetNewState aNode aNodeState qn

    //return whether next state at the corner is 'rel' to current
    rel newState aNodeState


let CanStrictlyIncrease (aNode: QN.node) aNodeState (qn: QN.node list) (bounds: Map<QN.var, int*int>) =
    template aNode aNodeState qn bounds QN.Act gt

let CanStrictlyDecrease (aNode: QN.node) aNodeState (qn: QN.node list) (bounds: Map<QN.var, int*int>) =
    template aNode aNodeState qn bounds QN.Inh lt

let AlwaysStrictlyIncreases (aNode: QN.node) aNodeState (qn: QN.node list) (bounds: Map<QN.var, int*int>) =
    template aNode aNodeState qn bounds QN.Inh gt

let AlwaysStrictlyDecreases (aNode: QN.node) aNodeState (qn: QN.node list) (bounds: Map<QN.var, int*int>) =
    template aNode aNodeState qn bounds QN.Act lt