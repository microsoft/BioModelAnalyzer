module Shrink

open System

/// Return updated bounds of var using curBounds of its inputs
let rec UpdateVarBounds (node: QN.node) ranges (curBounds: Map<QN.var, int*int>) =
    let nodeLower = (fst curBounds.[node.var]) 
    let nodeUpper = (snd curBounds.[node.var])
    let WithinOld (newLower, newUpper) =
        ((max (min newLower nodeUpper) nodeLower), (max (min newUpper nodeUpper) nodeLower))
    if (fst curBounds.[node.var]) < (snd curBounds.[node.var]) then
        Map.add node.var
                (WithinOld 
                    (FNewLemmas.tighten node.var ranges node.f nodeLower nodeUpper node.inputs curBounds))
               // ((Oracle.GetNewLowerBound node nodeLower ranges curBounds),
                //    (Oracle.GetNewUpperBound node nodeUpper ranges curBounds))// already within [nodeLower, nodeUpper]
                curBounds
    else
        curBounds


let NoChangeInBoundsForNode (node : QN.node) (oldBounds : Map<QN.var, int*int>) (newBounds : Map<QN.var, int*int>) =
    oldBounds.[node.var] = newBounds.[node.var]

let NoChangeInBounds (qn : QN.node list) (oldBounds : Map<QN.var, int*int>) (newBounds : Map<QN.var, int*int>) =
    List.forall (fun node -> NoChangeInBoundsForNode node oldBounds newBounds) qn


let MutateExpr (qn : QN.node list) (srcNodeVar : QN.var) (dstNodeVar : QN.var) (nature : QN.nature)=
    let dstNode = QN.get_node_from_var dstNodeVar qn

    if srcNodeVar=dstNodeVar || List.exists (fun x -> x = srcNodeVar) dstNode.inputs then
        dstNode.f
    else
        if dstNode.defualtF then
            let posInputs = List.filter (fun inp -> dstNode.nature.[inp] = QN.Act) dstNode.inputs
            let negInputs = List.filter (fun inp -> dstNode.nature.[inp] = QN.Inh) dstNode.inputs

            let posInputs = if nature = QN.Act then srcNodeVar :: posInputs else posInputs
            let negInputs = if nature = QN.Inh then srcNodeVar :: negInputs else negInputs

            let (min, max) = dstNode.range

            let newExpr =
                match posInputs, negInputs with
                | [], [] -> Expr.Const(min)
                | [], _  -> Expr.Minus(Expr.Const(max), Expr.Ave(List.map (fun o -> Expr.Var(o)) negInputs))
                | _ , [] -> Expr.Ave(List.map (fun i -> Expr.Var(i)) posInputs)
                | _ , _  -> Expr.Minus(Expr.Ave(List.map (fun i -> Expr.Var(i)) posInputs),
                                        Expr.Ave(List.map (fun o -> Expr.Var(o)) negInputs))
            newExpr
        else
            let numInputs = List.length dstNode.inputs
            Expr.Div(Expr.Plus(Expr.Times(dstNode.f, Expr.Const(numInputs)), Expr.Var(srcNodeVar)), Expr.Const(numInputs+1))

let FindStabilizingEdgeScores (qn : QN.node list) ranges (bounds : Map<QN.var, int*int>) 
                (qnStrategy : Map<QN.var, GGraph.Strategy<QN.var>>) (qnWTO : GGraph.Component<QN.var> list) = 
    List.fold
        (fun scores (src, dst, ntr)->
            let mutatedExpr = MutateExpr qn src dst ntr
            let dstNode = QN.get_node_from_var dst qn
            let newBounds = UpdateVarBounds 
                                { dstNode with f=mutatedExpr; inputs = src::dstNode.inputs; nature= Map.add src ntr dstNode.nature } 
                                ranges bounds
            let depthScore = GGraph.GetDepthOfVertexInWTO dst qnWTO
            if NoChangeInBoundsForNode dstNode bounds newBounds then
                ((src, dst, ntr), (5, depthScore)) :: scores
            else
                if qnStrategy.[dst].isHead then
                    if dstNode.defualtF then
                        ((src, dst, ntr), (1, depthScore)) :: scores
                    else
                        ((src, dst, ntr), (2, depthScore)) :: scores
                else
                    if dstNode.defualtF then
                        ((src, dst, ntr), (3, depthScore)) :: scores
                    else
                        ((src, dst, ntr), (4, depthScore)) :: scores)
        []
        [ for src in qn do for dst in qn do for ntr in [QN.Act; QN.Inh] -> (src.var, dst.var, ntr) ]
                    
let printEdgeScores (scores : List<(QN.var*QN.var*QN.nature)*(int*int)>)=
    let scores = List.sortBy (fun (k, v) -> v) scores
    let counter = ref 0
    for ((s, d, n), (v1, v2)) in scores do
        if !counter < 30 then
            printf "%d -> %A -> %d : (%d, %d)\n" s n d v1 v2
            incr counter


let FindUnstableHead (qnStrategy: Map<QN.var, GGraph.Strategy<QN.var>>) qnStartPoint (bounds : Map<QN.var, int*int>)= 
    let mutable visited = Set.empty

    let mutable curNodeVarOption = qnStartPoint
    let mutable unstableHead = None

    while not (Option.isNone curNodeVarOption) do
        let curNodeVar= Option.get curNodeVarOption
        
        if qnStrategy.[curNodeVar].isHead then
            if visited.Contains curNodeVar then
                curNodeVarOption <- qnStrategy.[curNodeVar].exit
            else
                visited <- Set.add curNodeVar visited
                let (lower, upper) = bounds.[curNodeVar] 
                if not (lower = upper) then
                    unstableHead <- Some curNodeVar
                    curNodeVarOption <- None
                else
                    curNodeVarOption <- qnStrategy.[curNodeVar].next
        else
            curNodeVarOption <- qnStrategy.[curNodeVar].next

    unstableHead

let rec Shrink (qn : QN.node list) qnStrategy qnStartPoint startFrontier startBounds outputs=

    let ranges = Map.ofList [for node in qn -> (node.var, node.range)]
    let inputs = Map.ofList [for node in qn -> (node.var, node.inputs)]

    let mutable bounds = startBounds
    
    let mutable frontier:Set<QN.var> = startFrontier

    

    let mutable curNodeVarOption = qnStartPoint
    let mutable visitedHead = Map.filter (fun _ (strategy : GGraph.Strategy<QN.var>) -> strategy.isHead) qnStrategy
                                |> Map.fold (fun map nodeVar _ -> Map.add nodeVar false map) Map.empty
    let updateCounter = ref 0
    while not (Option.isNone curNodeVarOption) do
        
        let curNodeVar = (Option.get curNodeVarOption)
        let curNode = QN.get_node_from_var curNodeVar qn

        let mutable noChange = true

        if frontier.Contains curNode.var then 
            if Log.level(2) then Log.log_debug("Updating node" + (QN.str_of_node curNode) )
            if Log.level(2) then Log.log_debug("Bounds={" + (QN.str_of_range qn bounds) + "}")
            incr updateCounter
            let newBounds = UpdateVarBounds curNode ranges bounds 
                        
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
    
    if Log.level(1) then Log.log_debug ("Updated variables " + (string) !updateCounter + " times.")
    if Log.level(1) then Log.log_debug ("Evaluated " + (string) (Expr.GetNumExprsEvaled()) + " exprs.")
    if Log.level(1) then Log.log_debug ("bounds={" + (QN.str_of_range qn bounds) + "}")
    if Map.forall (fun _ (lower,upper) -> upper = lower) bounds then
        if Log.level(1) then Log.log_debug ("Stabilizing")

        
        (*
        let env0 = (Map.fold (fun map v (lower, _) -> Map.add v lower map) Map.empty bounds)
        let mutable allEnvs = Map.add env0 0 Map.empty
        let mutable env = env0
        let mutable i = 0
        let steps = 10000
        while i<steps do
             if Log.level(2) then Log.log_debug (sprintf "Step: %d, Env %s" i (Expr.str_of_env env))
             env <- (Simulate.tick qn env)
             if allEnvs.ContainsKey env then 
                if Log.level(1) then Log.log_debug (sprintf "Step %d, Back To %d, LOOP FOUND!!!!!!!!!!!!!!!!!! at %s" i allEnvs.[env] (Expr.str_of_env env))
                i <- steps
             else
                allEnvs <- Map.add env i allEnvs
                i <- i+1
        if Log.level(1) then Log.log_debug ("Press any key.. ")
        ignore (Console.ReadKey())
        *)
    else
        if Log.level(1) then Log.log_debug ("Not-Stabilizing")
        


        (*
        if Log.level(1) then Log.log_debug ("Now simulating from arbitrary point to find cycle")
        let env0 = (Map.fold (fun map v (lower, _) -> Map.add v lower map) Map.empty bounds)
        let mutable allEnvs = Map.add env0 0 Map.empty
        let mutable env = env0
        let mutable i = 0
        let steps = 10000
        while i<steps do
             if Log.level(2) then Log.log_debug (sprintf "Step: %d, Env %s" i (Expr.str_of_env env))
             env <- (Simulate.tick qn env)
             if allEnvs.ContainsKey env then 
                if Log.level(1) then Log.log_debug (sprintf "Step %d, Back To %d, LOOP FOUND!!!!!!!!!!!!!!!!!! at %s" i allEnvs.[env] (Expr.str_of_env env))
                i <- steps
             else
                allEnvs <- Map.add env i allEnvs
                i <- i+1
        if Log.level(1) then Log.log_debug ("Press any key.. ")
        ignore (Console.ReadKey())


        let (cutNodeVar, cutAt) = Cut.ExploreCuts qn ranges bounds
        if Log.level(1) then Log.log_debug (sprintf "CutNode %d, CutPoint %d" cutNodeVar cutAt)
        
       // if Log.level(1) then Log.log_debug ("Press any key.. ")
        //ignore (Console.ReadKey())


        let newFrontier = (Map.find cutNodeVar outputs)
        ignore (Shrink qn qnStrategy qnStartPoint newFrontier (Map.add cutNodeVar ((fst bounds.[cutNodeVar]), cutAt) bounds))
        ignore (Shrink qn qnStrategy qnStartPoint newFrontier (Map.add cutNodeVar (cutAt+1, (snd bounds.[cutNodeVar])) bounds))
        *)
    bounds
    


let AddEdgeInQN (src, dst, ntr) qn = 
    let dstNode = QN.get_node_from_var dst qn
    let mutatedExpr = MutateExpr qn src dst ntr
    let qn = List.filter (fun (node : QN.node) -> not(node.var = dst)) qn
    let qn = 
        {dstNode with f=mutatedExpr; inputs = src::dstNode.inputs; nature= Map.add src ntr dstNode.nature} :: qn
    qn

let rec CallShrinkInternal (qn : QN.node list) bounds frontier qnGraph reCalc =
    let ranges = Map.ofList [for node in qn -> (node.var, node.range)]
    let inputs = Map.ofList [for node in qn -> (node.var, node.inputs)]

    let mutable outputs = Map.ofList [for node in qn -> (node.var, Set.empty)]

    for node in qn do
        for input in node.inputs do
            let curr_outs = outputs.[input]
            outputs <- Map.add input (curr_outs.Add node.var) outputs

    let qnGraph' =
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
    let qnGraph = if reCalc then qnGraph' else qnGraph
    let (qnStrategy, qnStartPoint) = GGraph.GetRecursiveStrategy qnGraph
    let qnWTO = GGraph.GetWeakTopologicalOrder qnGraph
    if Log.level(1) then Log.log_debug("WTO:" + GGraph.Stringify (qnWTO) string)
    let shrunkBounds = Shrink qn qnStrategy qnStartPoint frontier bounds outputs
    if Map.forall (fun _ (lower,upper) -> upper = lower) shrunkBounds then
        if Log.level(1) then Log.log_debug("Stabilized")
        for node in qn do
            if Log.level(2) then Log.log_debug (QN.str_of_node node)
    else
        let scores = FindStabilizingEdgeScores qn ranges shrunkBounds qnStrategy qnWTO
        let (newEdge, score) = List.minBy (fun (k, (s, d)) -> (s,d)) scores
        
        let (s, d, n) = newEdge 
        if Log.level(1) then 
            Log.log_debug(sprintf "%d -> %A -> %d : (%d, %d)\n" s n d (fst score) (snd score))
            ignore(Console.ReadKey())

        let qn = AddEdgeInQN newEdge qn

        CallShrinkInternal qn shrunkBounds (Set.add d Set.empty) qnGraph ((fst score)>=2)


let CallShrink (qn : QN.node list) = 

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
    
    let mutable frontier = 
        Set.ofList 
            (List.fold 
                (fun bb (n:QN.node) -> 
                    if (Expr.is_a_const ranges n.f) then bb
                    else n.var :: bb)
                []
                qn)
    
    
    if Log.level(1) then Log.log_debug("StartBounds={" + (QN.str_of_range qn bounds) + "}")

    
    CallShrinkInternal qn bounds frontier GGraph.Empty<QN.var> true