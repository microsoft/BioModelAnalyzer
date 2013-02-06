(* Copyright (c) Microsoft Corporation. All rights reserved. *)
module Marshal 

open System.Xml
open System.Xml.Linq
open Microsoft.FSharp.Text.Lexing
//
open ExprLex
open ExprParse

//
// XML->QN.Model parser. 
//
type var = int 
type ui_variable = {  Vid: var; Vname: string; Vfr: int; Vto: int; Vf: Expr.expr option } 
type RelTy = RTActivator | RTInhibitor
type ui_rel = { Rid: var; Rfr: int; Rto: int; Rrel_ty: RelTy }

exception MarshalInFailed of var * string

// string->XName. 
let xn s = XName.Get s

let model_of_xml (xd:XDocument) = 
    
    // Get vars
    let vv = xd.Element(xn "AnalysisInput").Element(xn "Variables").Elements(xn "Variable")
    let vars = 
        seq { for v in vv do             
                // watch out for null values! {Name,Range,...} 
                let id = try (int)(v.Attribute(xn "Id").Value) with _ -> raise(MarshalInFailed(-1,"Bad Variable Id"))
                let name = try v.Element(xn "Name").Value with _ -> raise(MarshalInFailed(id,"Bad Name"))
                let min = try (int) (v.Element(xn "RangeFrom").Value) with _ -> raise(MarshalInFailed(id,"Bad RangeFrom"))
                let max = try (int) (v.Element(xn "RangeTo").Value) with _ -> raise(MarshalInFailed(id,"Bad RangeTo"))
                // [t] can be None, in which case we'll synthesize a default T in [qn_map] later. 
                let t = try Some ((string) (v.Element(xn "Function").Value)) with _ -> None                
                let parse_err f exn  = 
                    "Failed to parse " + name + "'s function: " + f + ". " + 
                    "Exception: " + (string)exn + ". " + 
                    "Will use default function."
                let exn_msg f = "Failed to parse " + name + "'s transfer function" + f
                let f = 
                        match t with 
                        | Some t when t="" -> None
                        | Some t -> 
                            //Log.log_debug ("Trying to parse t:"+t)
                            try 
                                let lexbuf = LexBuffer<_>.FromString(t) 
                                let f = ExprParse.func ExprLex.tokenize lexbuf 
                                //Log.log_debug ("...OK, got a f:" + (Expr.str_of_expr f))
                                Some f
                            with e -> 
                                Log.log_error(parse_err t e)
                                raise(MarshalInFailed(id,exn_msg t))
                        | None -> None 
                yield { Vid= id; Vname= name; Vfr= min; Vto= max; Vf= f } }

    let vars_map = Seq.fold (fun map (v:ui_variable) -> Map.add v.Vid v map) Map.empty vars 

    // Get inputs
    let rr = xd.Element(xn "AnalysisInput").Element(xn "Relationships").Elements(xn "Relationship")
    let io = seq { for r in rr do 
                    let id = 
                        try (int)(r.Attribute(xn "Id").Value)
                        with _ -> raise(MarshalInFailed(-1,"Bad Relationship Id"))
                    let source = 
                        try (int)(r.Element(xn "FromVariableId").Value)
                        with _ -> raise(MarshalInFailed(id,"Bad FromVariableId"))
                    let target = 
                        try (int)(r.Element(xn "ToVariableId").Value)
                        with _ -> raise(MarshalInFailed(id,"Bad ToVariableId"))
                    let parsed_rel = (r.Element(xn "Type")).Value
                    let rel_ty = 
                        match parsed_rel with 
                        | "Activator" -> RTActivator 
                        | "Inhibitor" -> RTInhibitor 
                        | _ -> raise(MarshalInFailed(id,"Bad relationship descriptor"))
                    yield { Rid= id; Rfr= source; Rto= target; Rrel_ty=rel_ty } }
    // The inputs to a variable. "x depends upon ({act0,act1,...}, {inh0,inh1,...})"
    let inputs = Seq.fold (fun map (v:ui_variable) -> Map.add v.Vid ([],[]) map) Map.empty vars 
    let inputs' = 
        Seq.fold 
            (fun inputs (io:ui_rel) -> 
                let v = io.Rto 
                let v_acts, v_inhs = Map.find v inputs 
                Map.add 
                    v
                    (match io.Rrel_ty with 
                     | RTActivator -> io.Rfr :: v_acts, v_inhs            
                     | RTInhibitor -> v_acts          , io.Rfr :: v_inhs)
                    inputs) 
            inputs 
            io 

   // v->f map
    let v_to_f = Seq.fold (fun map (v:ui_variable) -> Map.add v.Vid v.Vf map) Map.empty vars 
    let v_to_frto = Seq.fold (fun map (v:ui_variable) -> Map.add v.Vid (v.Vfr,v.Vto) map) Map.empty vars

    // var->node map
    // This could be simpler if we'd worked out inputs (and so been able to 
    // synthesize default target functions) *before* making the vars_map. 
    let qn_map = 
        Map.map 
            (fun v (ii,oo) -> 
                let (min,max) = Map.find v v_to_frto
                let v = Map.find v vars_map 
                let t = 
                    // Use f if defined, or else synthesize a default one. 
                    match v.Vf with 
                    | Some f -> f
                    | None -> Expr.Max(Expr.Const(0), 
                                       Expr.Minus(Expr.Ave(List.map (fun i -> Expr.Var(i)) ii), 
                                                  Expr.Ave(List.map (fun o -> Expr.Var(o)) oo)))
// SI: Nir's default functions. (Change results for UnitTests #31, #32, #34. 
//                    | None -> 
//                        match ii, oo with 
//                        | [], [] -> Expr.Const(min)
//                        | [], _  -> Expr.Minus(Expr.Const(max), Expr.Ave(List.map (fun o -> Expr.Var(o)) oo))
//                        | _ , [] -> Expr.Min(Expr.Const(max), Expr.Ave(List.map (fun i -> Expr.Var(i)) ii))
//                        | _ , _  -> 
//                            Expr.Max(Expr.Const(min),
//                                     Expr.Minus(Expr.Ave(List.map (fun i -> Expr.Var(i)) ii),
//                                                Expr.Ave(List.map (fun o -> Expr.Var(o)) oo)))
                { QN.var= v.Vid; QN.range= (v.Vfr,v.Vto); QN.f= t; QN.inputs= ii@oo; QN.name= v.Vname } )
            inputs'
    
    // Final result 
    let qn = Map.toList qn_map |> List.map (fun (_v,n) -> n) 
    QN.qn_wf qn 
    qn


//    <AnalysisOutput>
//        <Status>Error</Status>
//        <Error Id="0" Msg="err_msg_0" />
//        ...
//        <Error Id="n" Msg="err_msg_n" />
//    </AnalysisOutput>
let xml_of_error (id:var) (msg:string) = 

    let doc = new XDocument()

    let root = new XElement(xn "AnalysisOutput")
    doc.AddFirst(root)

    let status = new XElement(xn "Status", "Error")
    root.Add(status)
    let err = new XElement(xn "Error", new XAttribute(xn "Id",id), new XAttribute(xn "Msg",msg))
    root.Add(err)

    doc 


//
// Result.result->XML printer.
//

// ctor for Variable, Range elt 
let mk_Variable id x = 
        let xv = new XElement(xn "Variable", new XAttribute(xn "Id",id))
        let x = new XElement(xn "Value", (string)x)
        xv.Add(x)
        xv

let mk_Variable_range id lo hi = 
        let attr = [|new XAttribute(xn "Id",id); new XAttribute(xn "Lo",lo); new XAttribute(xn "Hi",hi)|]
        let xv = new XElement(xn "Variable", attr)
        xv
 

let xml_of_stability_result (r:Result.stability_result) = 

    let add_a_tick (root:XElement) (time:int) (bounds:Map<QN.var,int*int>) = 
        let tick = new XElement (xn "Tick")
        root.Add(tick)
        let t = new XElement(xn "Time", (string)time)
        tick.Add(t)
        let variables = new XElement(xn "Variables")
        tick.Add(variables)
        let vv = Map.fold (fun vv k (lo,hi) -> (mk_Variable_range k lo hi)::vv) [] bounds
        variables.Add(vv)        

    let doc = new XDocument()
    let root = new XElement(xn "AnalysisOutput")
    doc.AddFirst(root)

    match r with 
    | Result.SRNotStabilizing(bounds_history) -> 
        root.Add(new XElement(xn "Status", "NotStabilizing"))
        List.iter (fun (t,bounds) -> add_a_tick root t bounds) bounds_history
    | Result.SRStabilizing(bounds_history) -> 
        root.Add(new XElement(xn "Status", "Stabilizing"))
        List.iter (fun (t,bounds) -> add_a_tick root t bounds) bounds_history

    doc

let xml_of_cex_result (r:Result.cex_result) = 
    
    let declare_var id x = 
        let xv = new XElement(xn "Variable", new XAttribute(xn "Id",id), new XAttribute(xn "Value",x))
        xv

    let doc = new XDocument()
    let root = new XElement(xn "AnalysisOutput")
    doc.AddFirst(root)

    match r with 
    | Result.CExBifurcation(fix1,fix2) ->
        let status = new XElement(xn "Status", "Bifurcation")
        root.Add(status)
        let vv1 = new XElement(xn "Variables")
        root.Add(vv1)
        let v1 = Map.fold (fun vv k v -> (declare_var k v)::vv) [] fix1
        vv1.Add([v1])
        let vv2 = new XElement(xn "Variables")
        root.Add(vv2)
        let v2 = Map.fold (fun vv k v -> (declare_var k v)::vv) [] fix2
        vv2.Add([v2])    
    | Result.CExCycle(fix) -> 
        let status = new XElement(xn "Status", "Cycle")
        root.Add(status)       
        let vv = new XElement(xn "Variables")
        root.Add(vv)
        let v = Map.fold (fun vv k v -> (declare_var k v)::vv) [] fix        
        vv.Add(v)
    | Result.CExFixpoint(fix) -> 
        let status = new XElement(xn "Status", "Fixpoint")
        root.Add(status)        
        let vv = new XElement(xn "Variables")
        root.Add(vv)
        let v = Map.fold (fun vv k v -> (declare_var k v)::vv) [] fix
        vv.Add(v)
    | Result.CExUnknown -> 
        let status = new XElement(xn "Status", "Unknown")
        root.Add(status)        
    doc 


// Result parsers
let stabilizing_result_of_xml (xd:XDocument) = 
    let parse_tick (tick:XElement) = 
        let time = (int)(tick.Element(xn "Time").Value)
        let variables = tick.Element(xn "Variables").Elements(xn "Variable")
        let bounds =
            Seq.fold    
                (fun (bb:QN.interval) (v:XElement) ->
                    let id = (int)(v.Attribute(xn "Id").Value)
                    let lo = (int)(v.Attribute(xn "Lo").Value)
                    let hi = (int)(v.Attribute(xn "Hi").Value)
                    Map.add id (lo,hi) bb)
                Map.empty
                variables
        (time,bounds)
    let output = xd.Element(xn "AnalysisOutput")
    let status = output.Element(xn "Status").Value
    match status with
    | "Stabilizing" -> 
        let ticks = output.Elements(xn "Tick")
        let history = Seq.fold (fun h tick -> (parse_tick tick) :: h) [] ticks
        Result.SRStabilizing(history)
    | "NotStabilizing" -> 
        let ticks = output.Elements(xn "Tick")
        let history = Seq.fold (fun h tick -> (parse_tick tick) :: h) [] ticks
        Result.SRNotStabilizing(history)
    | x -> raise(MarshalInFailed(-1,"Bad status descriptor: "+x))

let cex_result_of_xml (xd:XDocument) = 
    let parse_variable (v:XElement) = 
        let id = (string)(v.Attribute(xn "Id").Value)
        let value = (int)(v.Attribute(xn "Value").Value)
        (id,value)
    let output = xd.Element(xn "AnalysisOutput")
    let status = output.Element(xn "Status").Value
    match status with
    | "Bifurcation" -> 
        let variables = output.Elements(xn "Variables")
        match Seq.toList variables with 
        | [ fix1_x; fix2_x ] -> 
            let vars1 = fix1_x.Elements(xn "Variable")
            let fix1 = 
                Seq.fold 
                    (fun m v -> let (id,value) = (parse_variable v) in Map.add id value m)
                    Map.empty
                    vars1
            let vars2 = fix2_x.Elements(xn "Variable")
            let fix2 = 
                Seq.fold 
                    (fun m v -> let (id,value) = (parse_variable v) in Map.add id value m)
                    Map.empty
                    vars2
            Result.CExBifurcation(fix1,fix2)
        | _ -> raise(MarshalInFailed(-1,"Bifurcation must have two Variables elts."))
    | "Cycle" -> 
        let variables = output.Elements(xn "Variables")
        match Seq.toList variables with 
        | [ fix_x ] -> 
            let vars = fix_x.Elements(xn "Variable")
            let fix = 
                Seq.fold 
                    (fun m v -> let (id,value) = (parse_variable v) in Map.add id value m)
                    Map.empty
                    vars
            Result.CExCycle(fix)
        | _ -> raise(MarshalInFailed(-1,"Cycle must have one Variables elt."))
    | "Fixpoint" -> 
        let variables = output.Elements(xn "Variables")
        match Seq.toList variables with 
        | [ fix_x ] -> 
            let vars = fix_x.Elements(xn "Variable")
            let fix = 
                Seq.fold 
                    (fun m v -> let (id,value) = (parse_variable v) in Map.add id value m)
                    Map.empty
                    vars
            Result.CExFixpoint(fix)
        | _ -> raise(MarshalInFailed(-1,"Cycle must have one Variables elt."))        
    | "Unknown" -> Result.CExUnknown
    | x -> raise(MarshalInFailed(-1,"Bad status descriptor: "+x))
