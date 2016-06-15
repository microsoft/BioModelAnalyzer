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

///One-step, asynchronous. Returns list of possible successors
let asyncTick (qn:QN.qn) (state:Map<QN.var,int>) = 
    let state' = List.map (fun (node:QN.node) ->    let var = state.[node.var]
                                                    let var' = individualVariableTick qn state node.var
                                                    if var=var' then None else
                                                    Some(state.Add(node.var,var'))                      ) qn
                 |> List.filter (fun i -> i<>None )
                 |> List.map (fun (Some(i)) -> i)
    state'

//A fixpoint is a self loop; an EndComponent is a SCC in async space; Unknown is a set of states which may or may not be an EndComponent
type asyncEndComponent = FixPoint of Map<QN.var,int> | EndComponent of Map<QN.var,int> list  

//Apply a depth first search for a fix point from a state in async space. Returns asyncEndComponent
let dfsAsyncFixPoint (qn:QN.qn) (state:Map<QN.var,int>) =
    let rec join a b =
        match a with
        | [] -> b
        | head::tail -> join tail (head::b)
    //discovered is a list of visited states
    let rec core (qn:QN.qn) (state:Map<QN.var,int>) discovered = 
        let discoveredList = match discovered with EndComponent(n) -> n
        let discovered' = EndComponent(state::discoveredList)
        let state' = asyncTick qn state
        match state' with
        //No successors -> found a fix point
        | [] -> FixPoint(state) 
        | _ ->  let state' = List.filter (fun i -> not (List.exists (fun j -> i=j) discoveredList) ) state' //ignore states we've seen before
                //Two options- I have no new successors, or I have some
                match state' with
                | [] -> discovered'
                | _ ->  List.fold (fun acc s -> match acc with
                                                | FixPoint(_)   -> acc 
                                                | EndComponent(a)->    let result = (core qn s acc)
                                                                       match result with
                                                                       | FixPoint(_) -> result
                                                                       | EndComponent(states'') -> EndComponent(join states'' a)
                                                                       )
                                                                                         discovered' state'
    
    core qn state (EndComponent([]))

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

let simulate_up_to_loop (qn:QN.qn) (initial_value:Map<QN.var,int>) =
    let rec create_loop state acc =
        if (List.exists (fun elem -> elem = state) acc) then (state,acc)
        else
            let next_state = simulate qn state
            create_loop next_state (List.append acc ([state]))

    let (last_state, simulation) = create_loop  initial_value []

    let loop_closure = List.findIndex (fun elem -> elem = last_state) simulation
    (simulation, loop_closure)


