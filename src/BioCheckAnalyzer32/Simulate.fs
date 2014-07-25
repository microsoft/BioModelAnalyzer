module Simulate


/// One-step 
let tick (qn:QN.qn) (env_0:Map<QN.var,int>) = 
          
    let range = Map.ofList [for v in qn -> (v.var, v.range)]   
    let env' = 
        List.fold
            (fun env (v:QN.node) -> 
                // SI: is the range v's range? (Think so.)
                let s' = Expr.eval_expr v.var range v.f env_0
                let s = env_0.[v.var]
                if s' > s then Map.add v.var (s+1) env
                elif s' = s then Map.add v.var s env
                else Map.add v.var (s-1) env
            )
            Map.empty
            qn

    env'

///Returns the next state of a *single* variable 
let individualVariableTick (qn:QN.qn) (env_0:Map<QN.var,int>) (node:QN.var) = 
    let rec singleUpdate (qn:QN.qn) env_0 node range = 
        match qn with
        | topNode::remainder when topNode.var = node ->
                                                        let s' = Expr.eval_expr topNode.var range  topNode.f env_0
                                                        let s = env_0.[topNode.var]
                                                        if s' > s then s+1
                                                        elif s' = s then s
                                                        else s-1
        | topNode::remainder -> singleUpdate remainder env_0 node range
        | [] -> failwith "Node not in qn"
    let range = Map.ofList [for v in qn -> (v.var, v.range)]
    singleUpdate qn env_0 node range

let simulate (qn:QN.qn) (initial_values:Map<QN.var,int>) =
    tick qn initial_values


// Run simulation 'step' ticks. Return each step. 
let simulate_many_eager qn v0 steps = 
    let rec loop v t acc = 
        if (t=steps) then acc
        else
            let v' = simulate qn v
            loop v' (t+1) (v'::acc)
        
    let simulation = loop v0 1 []
    simulation // |> List.rev ? 

let simulate_many qn v0 steps = 
    Seq.unfold
        (fun (t,env) -> 
            if (t=steps) then None            
            else Some (env, (t+1,simulate qn env)))
        (1,v0)


