module Shrink

/// Return updated bounds of var using curBounds of its inputs
let rec UpdateVarBounds (node: QN.node) ranges inputs (curBounds: Map<QN.var, int*int>) =
    let nodeLower = (fst curBounds.[node.var]) 
    let nodeUpper = (snd curBounds.[node.var])
    let WithinOld (newLower, newUpper) =
        ((max (min newLower nodeUpper) nodeLower), (max (min newUpper nodeUpper) nodeLower))
    if (fst curBounds.[node.var]) < (snd curBounds.[node.var]) then
        Map.add node.var
                (*(WithinOld 
                    (FNewLemmas.tighten node.var ranges node.f nodeLower nodeUpper (Map.find node.var inputs) curBounds))*)
                ((Oracle.GetNewLowerBound node nodeLower ranges curBounds),
                    (Oracle.GetNewUpperBound node nodeUpper ranges curBounds))// already within [nodeLower, nodeUpper]
                curBounds
    else
        curBounds


let NoChangeInBoundsForNode (node : QN.node) (oldBounds : Map<QN.var, int*int>) (newBounds : Map<QN.var, int*int>) =
    oldBounds.[node.var] = newBounds.[node.var]

let NoChangeInBounds (qn : QN.node list) (oldBounds : Map<QN.var, int*int>) (newBounds : Map<QN.var, int*int>) =
    List.forall (fun node -> NoChangeInBoundsForNode node oldBounds newBounds) qn

let Shrink (qn : QN.node list) =

    let ranges = Map.ofList [for node in qn -> (node.var, node.range)]
    let inputs = Map.ofList [for node in qn -> (node.var, node.inputs)]

    let mutable bounds = 
        Map.ofList 
            (List.fold 
                (fun bb (n:QN.node) -> 
                    if (Expr.is_a_const ranges n.f) then 
                        let c = Expr.eval_expr n.var ranges n.f Map.empty
                        (n.var,(c,c)) :: bb
                    else (n.var, Map.find n.var ranges) :: bb)
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


    let mutable frontier = 
        Set.ofList 
            (List.fold 
                (fun bb (n:QN.node) -> 
                    if (Expr.is_a_const ranges n.f) then bb
                    else n.var :: bb)
                []
                qn)

    let mutable outputs = Map.ofList [for node in qn -> (node.var, Set.empty)]

    for node in qn do
        for input in node.inputs do
            let curr_outs = outputs.[input]
            outputs <- Map.add input (curr_outs.Add node.var) outputs


    

    if Log.if_default() then Log.log_default("WTO:" + GGraph.Stringify (GGraph.GetWeakTopologicalOrder qnGraph) string)
    if Log.if_default() then Log.log_default("StartBounds={" + (QN.str_of_range qn bounds) + "}")
       
    let mutable curNodeVarOption = qnStartPoint
    let mutable visitedHead = Map.filter (fun _ (strategy : GGraph.Strategy<QN.var>) -> strategy.isHead) qnStrategy
                                |> Map.fold (fun map nodeVar _ -> Map.add nodeVar false map) Map.empty
    let updateCounter = ref 0
    while not (Option.isNone curNodeVarOption) do
        
        let curNodeVar = (Option.get curNodeVarOption)
        let curNode = QN.get_node_from_var curNodeVar qn

        let mutable noChange = true

        if frontier.Contains curNode.var then 
            if Log.if_debug() then Log.log_debug("Updating node" + (QN.str_of_node curNode) )
            if Log.if_debug() then Log.log_debug("Bounds={" + (QN.str_of_range qn bounds) + "}")
            incr updateCounter
            let newBounds = UpdateVarBounds curNode ranges inputs bounds 
                        
            noChange <- NoChangeInBoundsForNode curNode bounds newBounds


            let newFrontier = 
                if noChange then
                    (Set.remove curNode.var frontier)
                else 
                    Set.fold (fun fr o -> Set.add o fr) (Set.remove curNode.var frontier) (Map.find curNode.var outputs)
            bounds <- newBounds
            frontier <- newFrontier
        
        
        
        let mutable newCurNodeVarOption = qnStrategy.[curNodeVar].next
        if qnStrategy.[curNodeVar].isHead then
            let visited = match Map.tryFind curNodeVar visitedHead with
                            | None -> failwithf "Head %A should have been found in the visitedHead map" curNodeVar 
                            | Some value -> value
            if visited then     
                if noChange then
                    newCurNodeVarOption <- qnStrategy.[curNodeVar].exit
                    visitedHead <- Map.add curNodeVar false visitedHead
            else
                    visitedHead <- Map.add curNodeVar true visitedHead
        
        curNodeVarOption <- newCurNodeVarOption
    
    if Log.if_default() then Log.log_default ("Updated variables " + (string) !updateCounter + " times.")
    if Log.if_default() then Log.log_default ("Evaluated " + (string) (Expr.GetNumExprsEvaled()) + " exprs.")
    if Log.if_default() then Log.log_default ("bounds={" + (QN.str_of_range qn bounds) + "}")
    if Map.forall (fun _ (lower,upper) -> upper = lower) bounds then
        if Log.if_default() then Log.log_default ("Stabilizing")
    else
        if Log.if_default() then Log.log_default ("Not-Stabilizing")

    bounds
    