module Simulator
open Automata

type formula = unit
type interp = Map<string, int>

let all_smt (f : formula) : Set<interp> = Set.empty

let add_interp (f : formula) (baseinterp : interp) : formula  = ()


let sim initform stepformula (rely : Automata<'istate,interp>) = 
    let normalize =
        let table = new System.Collections.Generic.Dictionary<interp * 'istate,  int>()
        let index = ref 0
        fun x -> 
            match table.TryGetValue x with
            | true, y -> y
            | false, _ -> 
                let r = !index
                table.Add(x,r)
                index := r + 1
                r
    let result = new SimpleAutomata<(interp * 'istate)>()

    for interp in all_smt initform do
        for index in rely.initialstates do
            result.addInitialState (normalize (interp,index))
            result.addState((normalize (interp,index)), (interp,index))
    
    let work_set = System.Collections.Concurrent.ConcurrentBag()

    for v in result.initialstates do 
        work_set.Add (result.value v)
    
    let mutable more, interp_index = work_set.TryTake()
    while more do
        let index = snd interp_index
        let interp = fst interp_index
        let new_indexs = rely.next(index)
        for new_interp in all_smt (add_interp (add_interp (stepformula) interp) (rely.value index))do
            for new_index in new_indexs do
                result.addEdge ((normalize (interp,index)), (normalize (new_interp,new_index)))
                if Set.contains (normalize (new_interp,new_index)) result.states then
                    ()
                else
                    result.addState ((normalize (new_interp,new_index)), (new_interp,new_index))
                    work_set.Add( (new_interp,new_index) )
        more <- work_set.TryTake(&interp_index)

    result