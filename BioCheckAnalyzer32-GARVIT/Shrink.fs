module Shrink

/// Return updated bounds of var using curBounds of its inputs
let rec UpdateVarBounds (node: QN.node) (qn: QN.node list) (curBounds: Map<QN.var, int*int>) =
    if Oracle.AlwaysStrictlyIncreases node (fst curBounds.[node.var]) qn curBounds then
        UpdateVarBounds node qn 
            (Map.remove node.var curBounds |> Map.add node.var ((fst curBounds.[node.var])+1, snd curBounds.[node.var]))
    elif Oracle.AlwaysStrictlyDecreases node (snd curBounds.[node.var]) qn curBounds then
        UpdateVarBounds node qn
            (Map.remove node.var curBounds |> Map.add node.var (fst curBounds.[node.var], (snd curBounds.[node.var])-1))
    else
        curBounds


let NoChangeInBoundsForNode (node : QN.node) (oldBounds : Map<QN.var, int*int>) (newBounds : Map<QN.var, int*int>) =
    oldBounds.[node.var] = newBounds.[node.var]

let NoChangeInBounds (qn : QN.node list) (oldBounds : Map<QN.var, int*int>) (newBounds : Map<QN.var, int*int>) =
    List.forall (fun node -> NoChangeInBoundsForNode node oldBounds newBounds) qn

let Shrink (qn : QN.node list) =

    let range = Map.ofList [for node in qn -> (node.var, node.range)]
    
    let mutable bounds = 
        Map.ofList 
            (List.fold 
                (fun bb (n:QN.node) -> 
                    if (Expr.is_a_const range n.f) then 
                        let c = Expr.eval_expr n.var range n.f Map.empty
                        (n.var,(c,c)) :: bb
                    else (n.var, Map.find n.var range) :: bb)
                []
                qn)

    let qnGraph =
        List.fold
            (fun graph (node : QN.node) ->
                List.fold
                    (fun graph input ->
                        GGraph.AddEdge input node.var graph)
                    graph
                    node.inputs)
            (List.fold
                (fun graph (node : QN.node) ->
                    GGraph.AddVertex node.var graph)
                GGraph.Empty<QN.var>
                qn)
            qn

    let (qnStrategy, qnStartPoint) = GGraph.GetRecursiveStrategy qnGraph

    let mutable curNodeVarOption = qnStartPoint

    while not (Option.isNone curNodeVarOption) do
        let curNodeVar = (Option.get curNodeVarOption)
        let curNode = QN.get_node_from_var curNodeVar qn
        let newBounds = UpdateVarBounds curNode qn bounds
        if qnStrategy.[curNodeVar].isHead then
            if NoChangeInBoundsForNode curNode bounds newBounds then
                curNodeVarOption <- qnStrategy.[curNodeVar].exit
            else 
                curNodeVarOption <- qnStrategy.[curNodeVar].next
        else
            curNodeVarOption <- qnStrategy.[curNodeVar].next
        bounds <- newBounds

    if Map.forall (fun _ (lower,upper) -> upper = lower) bounds then
        printfn "Stable at %A" bounds
    else
        printfn "Can't get further from %A" bounds
    