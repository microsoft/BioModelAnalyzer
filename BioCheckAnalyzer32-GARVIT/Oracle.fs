

module Oracle

let GetTransferState (aNode : QN.node) aNodeState ranges (bounds : Map<QN.var , int*int>) env =

    let tState = 
        env
        |> Map.ofList
        // add the value of var as aNodeState. replace in case var is an input to itself
        |> Map.add aNode.var aNodeState  
   
        |> Expr.eval_expr aNode.var ranges aNode.f 
    (min (max tState (fst bounds.[aNode.var])) (snd bounds.[aNode.var])) 

// lt and gt are prefix versions of < and  >
let lt x y = x < y
let gt x y = x > y


let Corner (aNode : QN.node) aNodeState (bounds : Map<QN.var, int*int>) nature =
    // create the environment to check whether var increases
    // select the corner point according to 'nature'
    [ for input in aNode.inputs do
        if aNode.nature.[input] = nature then
            yield (input, snd bounds.[input])
        else
            yield (input, fst bounds.[input])]

let TransferState (aNode : QN.node) aNodeState ranges (bounds : Map<QN.var, int*int>) nature =
    Corner aNode aNodeState bounds nature
    |> GetTransferState aNode aNodeState ranges bounds

    

let CanStrictlyIncrease (aNode : QN.node) aNodeState ranges (bounds : Map<QN.var, int*int>) =
    TransferState aNode aNodeState ranges bounds QN.Act > aNodeState

let CanStrictlyDecrease (aNode : QN.node) aNodeState ranges (bounds : Map<QN.var, int*int>) =
    TransferState aNode aNodeState ranges bounds QN.Inh < aNodeState

let AlwaysStrictlyIncreases (aNode : QN.node) aNodeState ranges (bounds : Map<QN.var, int*int>) =
    TransferState aNode aNodeState ranges bounds QN.Inh > aNodeState

let AlwaysStrictlyDecreases (aNode : QN.node) aNodeState ranges (bounds : Map<QN.var, int*int>) =
    TransferState aNode aNodeState ranges bounds QN.Act < aNodeState


let UntilStopsIncreasing (aNode : QN.node) aNodeState ranges (bounds : Map<QN.var, int*int>) =
    let corner = Corner aNode aNodeState bounds QN.Inh
    let mutable lb = aNodeState
    while (GetTransferState aNode lb ranges bounds corner) > lb do
        lb <- lb+1
    lb
    
let UntilStopsDecreasing (aNode : QN.node) aNodeState ranges (bounds : Map<QN.var, int*int>) =
    let corner = Corner aNode aNodeState bounds QN.Act
    let mutable ub = aNodeState
    while (GetTransferState aNode ub ranges bounds corner) < ub do
        ub <- ub-1
    ub

let GetNewLowerBound (aNode : QN.node) aNodeState ranges (bounds : Map<QN.var, int*int>) =
    if List.exists (fun elem -> elem=aNode.var) aNode.inputs then 
        UntilStopsIncreasing aNode aNodeState ranges bounds
    else
        TransferState aNode aNodeState ranges bounds QN.Inh

let GetNewUpperBound (aNode : QN.node) aNodeState ranges (bounds : Map<QN.var, int*int>) =
    if List.exists (fun elem -> elem=aNode.var) aNode.inputs then 
        UntilStopsDecreasing aNode aNodeState ranges bounds
    else
        TransferState aNode aNodeState ranges bounds QN.Act

