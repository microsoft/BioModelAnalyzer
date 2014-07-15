module Simulator
open Automata

type formula = unit
type interp = Map<string, int>

let all_smt (f : formula) : Set<interp> = Set.empty

let add_interp (f : formula) (baseinterp : interp) : formula  = ()

let sim initform stepformula (rely : Automata<'istate,interp>) = 
    let init_states = 
        seq {
            for interp in all_smt initform do
                for index in rely.initialstates do
                    yield (interp,index)
        } |> Set.ofSeq

    let reachable_states = ref init_states
    let work_set = System.Collections.Concurrent.ConcurrentBag()

    let trans_system = new System.Collections.Generic.Dictionary<interp * 'istate,  Set<interp * 'istate>>()
    let ts_add v1 v2 = 
        match trans_system.TryGetValue(v1) with
        | true, vs -> trans_system.[v1] <- Set.add v2 vs
        | false, _ -> trans_system.Add(v1, Set.singleton v2)

    for v in init_states do 
        work_set.Add v
    
    let mutable more, interp_index = work_set.TryTake()
    while more do
        let index = snd interp_index
        let interp = fst interp_index
        let new_indexs = rely.next(index)
        for new_interp in all_smt (add_interp (add_interp (stepformula) interp) (rely.value index))do
            for new_index in new_indexs do
                ts_add (interp,index) (new_interp,new_index)
                if Set.contains (new_interp,new_index) !reachable_states then
                    ()
                else
                    reachable_states := Set.add (new_interp,new_index) !reachable_states
                    work_set.Add( (new_interp,new_index) )
        more <- work_set.TryTake(&interp_index)
