module Automata

let spawnMachines (qn: QN.node list) (number:int) (rng:System.Random) (init0:string) =
    let rec maximum acc (s: int list) =
        match s with 
        | head::tail -> 
            if head > acc then maximum head tail
            else maximum acc tail
        | [] -> acc
    let max = maximum 0 [for v in qn ->
                            let (min,max) = v.range
                            max
                        ]
    
    let initState i0 = match i0 with
                        | "zero" -> List.fold (fun m (n:QN.node) -> Map.add n.var 0 m) Map.empty qn
                        | "random" -> List.fold (fun m (n:QN.node) -> Map.add n.var (rng.Next(0,(max+1))) m) Map.empty qn
                        | _ -> failwith "Cannot read the initial state of the machines"
    [for machine in [0..(number-1)] -> initState init0]

let updateMachines (qn: QN.node list) (machines: Map<QN.var,int> list) (threads:int) =
    //[for automata in machines -> Simulate.tick qn automata]
    machines
    |> Microsoft.FSharp.Collections.PSeq.ordered
    |> (fun pseq -> if threads > 0 then Microsoft.FSharp.Collections.PSeq.withDegreeOfParallelism threads pseq else pseq)
    |> Microsoft.FSharp.Collections.PSeq.map (fun (automata: Map<QN.var,int>) -> Simulate.tick qn automata)
    |> List.ofSeq