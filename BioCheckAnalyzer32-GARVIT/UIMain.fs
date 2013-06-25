(* Copyright (c) Microsoft Corporation. All rights reserved. *)
module UIMain

// Alternative entry point for UI, via IAnalyzer interface. 

open System.ComponentModel.Composition
open System.Xml
open System.Xml.Linq

open BioCheckAnalyzerCommon


[<Export(typeof<IAnalyzer2>)>]
type Analyzer2() = 
    // What kind of a class is it if it has no state? ;-)

    // "The worst-case cost of the stabilization proving procedure using F-New-
    // Lemmas is O(n^2 N^(d+1)) where the network has n variables, of maximal indegree
    // d (N^d results from generating input combinations)." 
    let complexity model = 
            // in case N is not the same for all, we just use the largest
            let N = List.fold (fun n_so_far (n:QN.node) -> max n_so_far (snd n.range)) 0 model
            let n = List.length model
            let d = List.fold (fun d_so_far (n:QN.node) -> max d_so_far (List.length (n.inputs))) 0 model
            (pown n 2 ) * (pown N (d+1))

    let find_cex (xml_model:XDocument) (xml_notstabilizing_result:XDocument) f_cex = 
            let model = Marshal.model_of_xml xml_model
            let instability_result = Marshal.stabilizing_result_of_xml xml_notstabilizing_result
            let (last_tick,last_bound) = 
                match instability_result with
                | Result.SRNotStabilizing(bounds_history) -> List.maxBy (fun (t,_bounds) -> t) bounds_history
                | _ -> raise(Marshal.MarshalInFailed(4,"FindCEx range argument is not NotStabilizing"))
            match f_cex model last_bound with
            | Some cex -> Marshal.xml_of_cex_result cex
            | None -> null 


    interface IAnalyzer2 with 
        member this.LoggingOn logger = 
            Log.register_log_service logger
            Log.log_debug (sprintf "BioCheck.fs, version %d.%d." Version.major Version.minor)
        
        member this.LoggingOff () = 
            Log.log_debug "Log off."
            Log.deregister_log_service ()

        member this.complexity (input_model:XDocument ) = 
            let network = Marshal.model_of_xml input_model    
            complexity network

        member this.checkStability(input_model:XDocument) = 
            try
                let network = Marshal.model_of_xml input_model
                let results = Stabilize.check_stability_lazy network
                let results = Seq.toArray results
                let result = Seq.nth ((Seq.length results) - 1) results 
                Marshal.xml_of_stability_result result
            with Marshal.MarshalInFailed(id,msg) -> Marshal.xml_of_error id msg                

        member this.findCExBifurcates(xml_model:XDocument, xml_notstabilizing_result:XDocument ) = 
            find_cex xml_model xml_notstabilizing_result Stabilize.find_cex_bifurcates            

        member this.findCExCycles(xml_model:XDocument, xml_notstabilizing_result:XDocument ) = 
            find_cex xml_model xml_notstabilizing_result Stabilize.find_cex_cycles

        member this.findCExFixpoint(xml_model:XDocument, xml_notstabilizing_result:XDocument ) = 
            find_cex xml_model xml_notstabilizing_result Stabilize.find_cex_fixpoint            

        member this.simulate_tick(xml_model:XDocument, env:System.Collections.Generic.Dictionary<int,int>) = 
            let qn = 
                try Marshal.model_of_xml xml_model
                with e -> raise(e)

            let mutable m = Map.empty
            for entry in env do
                m <- Map.add entry.Key entry.Value m
            // got an m
            let m' = Simulate.simulate qn m 
            
            let env' = new System.Collections.Generic.Dictionary<int,int>()
            Map.iter (fun k v ->  env'.Add(k,v)) m'
            env'
