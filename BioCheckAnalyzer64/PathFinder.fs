module PathFinder

(* 
Finds all the possible states between two attractors which exist under different 'constant' conditions
and confirms that none of the states can lead to alternative attractors

Inputs:     two qns representing the alternative environments (which are switched between)
            two attractor states (each associated with a specific QN
Outputs:    Either: states which fail to lead to one of the attractors
            Or:     a guarantee that no other attractors are accessible

The user should be aware if both states are stable this is unnecessary; this is only valuable when a
machine can move from an environment of stability to one of instability
*)

type locationLog = { forward: Map<QN.var,int> list ; backward: Map<QN.var,int> list ; safe: Map<QN.var,int> list }
type result = Success of locationLog | Failure of (Map<QN.var,int>)*(Map<QN.var,int>)

let routes (qnX: QN.node list) (qnY:QN.node list) (X:Map<QN.var,int>) (Y:Map<QN.var,int>) =
    let rec simulateToFix (qn: QN.node list) (state:Map<QN.var,int>) (acc:Map<QN.var,int> list) = 
        let state' = Simulate.tick qn state
        if (state'=state) then acc else simulateToFix qn state' (state'::acc)
    let reachDestination qn ini destination =
        match (simulateToFix qn ini [ini;]) with
        | fixpoint::trajectory when fixpoint=destination -> true
        | fixpoint::trajectory -> false //system escaped attractor
        | [] -> failwith "Empty simulation trajectory"
    let rec escapeAttractor (qn: QN.node list) (destination:Map<QN.var,int>) (acc:locationLog) (forward:bool) = 
        match (forward,acc.forward,acc.backward) with
        | (true,state::other_states,_)  -> match (simulateToFix qn state [state;]) with
                                           | fixpoint::trajectory when fixpoint=destination -> 
                                                //I need to collect the trajectory, and compare to my safe states
                                                //Anything that isn't in my safe states should be added to my backwards list
                                                let bkwd = (Set.ofList trajectory) + (Set.ofList acc.backward) - (Set.ofList acc.safe)
                                                            |> Set.toList
                                                Log.log_debug (sprintf "Successfully found the destination: %A states to go" other_states.Length)
                                                escapeAttractor qn destination {acc with forward=other_states; backward=bkwd} true
                                           | fixpoint::trajectory -> Failure (state,fixpoint)
                                           | [] -> failwith "Empty simulation trajectory"
        | (true,[],_)                   -> Log.log_debug (sprintf "No more states. %d new states found" acc.backward.Length); Success acc
        | (false,_,state::other_states) -> match (simulateToFix qn state [state;]) with
                                           | fixpoint::trajectory when fixpoint=destination -> 
                                                //I need to collect the trajectory, and compare to my safe states
                                                //Anything that isn't in my safe states should be added to my forwards list
                                                let frwd = (Set.ofList trajectory) + (Set.ofList acc.forward) - (Set.ofList acc.safe)
                                                            |> Set.toList
                                                Log.log_debug (sprintf "Successfully found the destination: %A states to go" other_states.Length)
                                                escapeAttractor qn destination {acc with backward=other_states; forward=frwd; safe = state::acc.safe } false
                                           | fixpoint::trajectory -> Failure (state,fixpoint)
                                           | [] -> failwith "Empty simulation trajectory"
        | (false,_,[])                  -> Log.log_debug (sprintf "No more states. %d new states found" acc.forward.Length); Success acc
    //The core of this has to run
    //0 Add X to the forward list
    //1 Generate new states from the forward list to Y (fail if Y is not encountered) and add to the backward list if they are not already in the safe list
    //2 Demonstrate that each of the backward states return to X and add to the safe list. Add the new states to the forward list if they are not already in the safe list
    //3 If forward list is not empty, goto 1
    let rec search (qnX: QN.node list) (X:Map<QN.var,int>) (qnY:QN.node list) (Y:Map<QN.var,int>) (L:locationLog) = 
        //Executions which should lead to Y are considered forward
        //Executions which should lead to X are considered backward
        match (L.forward=[],L.backward=[]) with 
        | (true,true)   -> Success L //Nothing left to do; return L with all the visited states
        | (false,false) -> failwith "Both lists have something to search for..."
        | (false,true)  -> //forward list searches
                            Log.log_debug "Searching forward paths"
                            match (escapeAttractor qnY Y L true) with
                            | Success L'  -> 
                                //Log.log_debug (sprintf "%A" L')
                                if (L' <> L) then search qnX X qnY Y L' else failwith "The LocationLog hasn't been changed by the search"
                            | Failure(a,b) -> Failure(a,b)
        | (true,false)  -> //backward list searches
                            Log.log_debug "Searching backward paths"
                            match (escapeAttractor qnX X L false) with
                            | Success L'  -> 
                                //Log.log_debug (sprintf "%A" L')
                                if (L' <> L) then search qnX X qnY Y L' else failwith "The LocationLog hasn't been changed by the search"
                            | Failure(a,b) -> Failure(a,b)
    search qnX X qnY Y {forward=[X;];backward=[];safe=[]}