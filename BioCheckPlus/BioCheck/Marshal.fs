(* Copyright (c) Microsoft Corporation. All rights reserved. *)
module Marshal 

open System.Xml
open System.Xml.Linq
//open Microsoft.FSharp.Text.Lexing
////
//open ExprLex
//open ExprParse

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
                let parse_error f line col msg = 
                    "Failed to parse " + name + "'s function: " + f + ". " +
                    "Exception: " + msg + ". " +
                    "Will use default function."
                let exn_msg _ = "Failed to parse " + name + "'s transfer function"
                let f = 
                        match t with 
                        | Some t when t="" -> None
                        | Some t -> 
                            Log.log_debug ("Trying to parse t:"+t)
                            match ParsecExpr.parse_expr t with
                            | ParsecExpr.ParseOK(f) -> Some f 
                            | ParsecExpr.ParseErr(err) -> 
                                Log.log_error(parse_error t err.line err.col err.msg) 
                                raise(MarshalInFailed(id,exn_msg t))

                        | None -> None 
                yield { Vid= id; Vname= name; Vfr= min; Vto= max; Vf= f } }

    // Work out range too
    let range = Seq.fold (fun range (v:ui_variable) -> Map.add v.Vid (v.Vfr,v.Vto) range) Map.empty vars
    
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
                let v = io.Rto // was io.Rfr
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
    let qn_map =  
        Map.map 
            (fun v (ii,oo) -> 
                let (min,max) = Map.find v v_to_frto
                let t = 
                    // Use f if defined, or else synthesize a default one. 
                   match Map.find v v_to_f with 
                    | Some f -> f
(*                    | None -> Expr.Max(Expr.Const(min), 
                                       Expr.Minus(Expr.Ave(List.map (fun i -> Expr.Var(i)) ii), 
                                                  Expr.Ave(List.map (fun o -> Expr.Var(o)) oo))) *)
                    | None -> 
                        if ((List.isEmpty ii) && (List.isEmpty oo)) then
                            Expr.Const(min)
                        elif (List.isEmpty ii) then
                            Expr.Minus(Expr.Const(max),
                                       Expr.Ave(List.map (fun o -> Expr.Var(o)) oo))
                        elif (List.isEmpty oo) then 
                            Expr.Min(Expr.Const(max),
                                     Expr.Ave(List.map (fun i -> Expr.Var(i)) ii))
                        else //Both lists are not empty
                            Expr.Max(Expr.Const(min),
                                     Expr.Minus(Expr.Ave(List.map (fun i -> Expr.Var(i)) ii),
                                                Expr.Ave(List.map (fun o -> Expr.Var(o)) oo)))
                // Make a QN.var (the name is just the id.
                { QN.var= v; QN.f= t; QN.inputs= ii@oo; QN.name= "v"+(string)v; QN.min = min; QN.max = max } ) 
            inputs'
    // Forget var key from qn_map.     
    let qn = Map.toList qn_map |> List.map (fun (_v,n) -> n) 
    
    QN.qn_wf qn 

    // Final result 
    qn, range


//
// Result.result->XML printer.
//

// ctor for Variable, Range elt 
let mk_Variable id x = 
        let xv = new XElement(xn "Variable", new XAttribute(xn "Id",id))
        let x = new XElement(xn "Value", (string)x)
        xv.Add(x)
        xv
let mk_Variable_range id x y = 
        let xv = new XElement(xn "Variable", new XAttribute(xn "Id",id))
        let x = new XElement(xn "Value", (string)x)
        let y = new XElement(xn "Value", (string)y)
        xv.Add([x;y])
        xv


let xml_of_result (r:Result.result) = 
    // Start making the doc
    let doc = new XDocument()

    // root elt
    let root = new XElement(xn "AnalysisOutput")
    doc.AddFirst(root)

    match r with 
    | Result.Stabilizing(fix) -> 
        // Status is Stabilizing
        let status = new XElement(xn "Status", "Stabilizing")
        root.Add(status)
        // The Variables elt holds the Variable list. 
        let vv = new XElement(xn "Variables")
        root.Add(vv)
        // The Variable list itself         
        let v = Map.fold (fun vv k v -> (mk_Variable k v) :: vv) [] fix 
        vv.Add(v)
    | Result.Bifurcation(fix1,fix2) ->
        // Status is Bifurcates
        let status = new XElement(xn "Status", "Bifurcation")
        root.Add(status)
        // fix1 variables 
        let vv1 = new XElement(xn "Variables")
        root.Add(vv1)
        let v1 = Map.fold (fun vv k v -> (mk_Variable k v)::vv) [] fix1       
        vv1.Add([v1])
        // fix2 variables 
        let vv2 = new XElement(xn "Variables")
        root.Add(vv2)
        let v2 = Map.fold (fun vv k v -> (mk_Variable k v)::vv) [] fix2
        vv2.Add([v2])

    | Result.Cycle(fix) -> 
        // Status is Cycle
        let status = new XElement(xn "Status", "Cycle")
        root.Add(status)       
        let vv = new XElement(xn "Variables")
        root.Add(vv)
        // The Variable list itself 
        let v = Map.fold (fun vv k v -> (mk_Variable k v)::vv) [] fix        
        vv.Add(v)
    | Result.Fixpoint(fix) -> 
        // Status is Fixpoint
        let status = new XElement(xn "Status", "Fixpoint")
        root.Add(status)        
        let vv = new XElement(xn "Variables")
        root.Add(vv)
        // The Variable list itself 
        let v = Map.fold (fun vv k v -> (mk_Variable k v)::vv) [] fix
        vv.Add(v)
    | Result.Unknown -> 
        // Status is Unknown
        let status = new XElement(xn "Status", "Unknown")
        root.Add(status)        
    doc 
    
// SI: cut-down the duplicate xml_of_result inlined in here, by
// making xdoc_o as optional parameter into xml_of_result* functions.  
let xml_of_result_steps (r:Result.result_steps) = 

    // Ctor the doc.
    let doc = new XDocument()

    // root elt
    let root = new XElement(xn "AnalysisOutput")
    doc.AddFirst(root)

    match r with 
    | Result.ResultStepping(var_range) -> 
        // Status is Bifurcates
        let status = new XElement(xn "Status", "TryingStabilizing")
        root.Add(status)
        let vv = new XElement(xn "Variables")
        root.Add(vv)
        let v = Map.fold (fun vv k (lo,hi) -> (mk_Variable_range k lo hi)::vv) [] var_range
        vv.Add(v)

    | Result.ResultLastStep(Result.Stabilizing(fix)) -> 
        // Status is Stabilizing
        let status = new XElement(xn "Status", "Stabilizing")
        root.Add(status)
        // The Variables elt holds the Variable list. 
        let vv = new XElement(xn "Variables")
        root.Add(vv)
        // The Variable list itself         
        let v = Map.fold (fun vv k v -> (mk_Variable k v) :: vv) [] fix 
        vv.Add(v)
    | Result.ResultLastStep(Result.Bifurcation(fix1,fix2)) ->
        // Status is Bifurcates
        let status = new XElement(xn "Status", "Bifurcation")
        root.Add(status)
        // fix1 variables 
        let vv1 = new XElement(xn "Variables")
        root.Add(vv1)
        let v1 = Map.fold (fun vv k v -> (mk_Variable k v)::vv) [] fix1
        vv1.Add([v1])
        // fix2 variables 
        let vv2 = new XElement(xn "Variables")
        root.Add(vv2)
        let v2 = Map.fold (fun vv k v -> (mk_Variable k v)::vv) [] fix2
        vv2.Add([v2])
        
    | Result.ResultLastStep(Result.Cycle(fix)) -> 
        // Status is Cycle
        let status = new XElement(xn "Status", "Cycle")
        root.Add(status)       
        let vv = new XElement(xn "Variables")
        root.Add(vv)
        // The Variable list itself 
        let v = Map.fold (fun vv k v -> (mk_Variable k v)::vv) [] fix        
        vv.Add(v)
    | Result.ResultLastStep(Result.Fixpoint(fix)) -> 
        // Status is Fixpoint
        let status = new XElement(xn "Status", "Fixpoint")
        root.Add(status)        
        let vv = new XElement(xn "Variables")
        root.Add(vv)
        // The Variable list itself 
        let v = Map.fold (fun vv k v -> (mk_Variable k v)::vv) [] fix
        vv.Add(v)
    | Result.ResultLastStep(Result.Unknown) -> 
        // Status is Unknown
        let status = new XElement(xn "Status", "Unknown")
        root.Add(status)        
    doc 


//
//  XML->Result.result parser
//

let parse_var (v:XElement) = 
    let id = v.Attribute(xn "Id").Value
    let value = try (int) (v.Element(xn "Value").Value) with _ -> raise(MarshalInFailed(-1,"Bad Value"))
    id,value
        
let result_of_xml (xd:XDocument) = 
    let output = xd.Element(xn "AnalysisOutput")
    let status = output.Element(xn "Status").Value    
    match status with 
    | "Stabilizing" ->
        let vv = output.Element(xn "Variables").Elements(xn "Variable")
        let vars = 
            Seq.fold 
                (fun map v -> let (id,value) = parse_var v in Map.add id value map)
                Map.empty
                vv
        Result.Stabilizing(vars)
    | "Bifurcation" ->
        let vv = output.Elements(xn "Variables")
        let fst,snd = 
            try Seq.nth 0 vv, Seq.nth 1 vv 
            with _ -> raise(MarshalInFailed(-1,"Bifurcation must have two Variables"))
        let fst = 
            let fst_vv = fst.Elements(xn "Variable")
            Seq.fold 
                (fun map v -> let (id,value) = parse_var v in Map.add id value map)
                Map.empty
                fst_vv
        let snd = 
            let snd_vv = snd.Elements(xn "Variable")
            Seq.fold 
                (fun map v -> let (id,value) = parse_var v in Map.add id value map)
                Map.empty
                snd_vv
        Result.Bifurcation(fst,snd)
    | "Cycle" -> 
        let vv = output.Element(xn "Variables").Elements(xn "Variable")
        let vars = 
            Seq.fold 
                (fun map v -> let (id,value) = parse_var v in Map.add id value map)
                Map.empty
                vv
        Result.Cycle(vars)
    | "Fixpoint" -> 
        let vv = output.Element(xn "Variables").Elements(xn "Variable")
        let vars = 
            Seq.fold 
                (fun map v -> let (id,value) = parse_var v in Map.add id value map)
                Map.empty
                vv
        Result.Fixpoint(vars)               
    | "Unknown" ->
        Result.Unknown
    | _ -> raise(MarshalInFailed(-1,"Bad status descriptor"))

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


