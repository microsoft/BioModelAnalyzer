(* Copyright (c) Microsoft Corporation. All rights reserved. *)
module Marshal

open System.Xml
open System.Xml.Linq

open BioModelAnalyzer

//
// XML->QN.Model parser.
//
type var = QN.var
type ui_variable = {  Vid: var; Vname: string; Vfr: int; Vto: int; Vf: Expr.expr option; Vnumber : QN.number; Vtags : (int*QN.cell) list }
type RelTy = RTActivator | RTInhibitor
type ui_rel = { Rid: var; Rfr: int; Rto: int; Rrel_ty: RelTy }

exception MarshalInFailed of var * string

// string->XName.
let xn s = XName.Get s

let maxNumber = ref 0 // must be updated before mk_number_safe is called
let mk_number_safe () =
   incr maxNumber
   !maxNumber

let model_of_xml (xd:XDocument) =
    // Get cells
    //let cc = try xd.Element(xn "AnalysisInput").Element(xn "Cells").Elements(xn "Cell") with _ -> Seq.empty
    let cc = 
        if ((xd.Element(xn "AnalysisInput").Element(xn "Cells") <> null) && (xd.Element(xn "AnalysisInput").Element(xn "Cells").Element(xn "Cell") <> null)) then
            xd.Element(xn "AnalysisInput").Element(xn "Cells").Elements(xn "Cell") 
        else Seq.empty 
            
    let ccNames = if Seq.isEmpty cc then ["SingleCell"] 
                  else [for cell in cc do yield try (string)(cell.Attribute(xn "Name").Value) with _ -> raise(MarshalInFailed(-1, "Bad Cell Name"))]

    // Get vars
    //let vv = xd.Element(xn "AnalysisInput").Element(xn "Variables").Elements(xn "Variable")
    let vv = 
        if ((xd.Element(xn "AnalysisInput").Element(xn "Variables") <> null)  && (xd.Element(xn "AnalysisInput").Element(xn "Variables").Element(xn "Variable") <> null)) then
            xd.Element(xn "AnalysisInput").Element(xn "Variables").Elements(xn "Variable")
        else Seq.empty

    // Get the max of all the variable ids to create safe ids for internal use
    maxNumber := seq { for v in vv do
                            let number = 
                                // try (int) (v.Element(xn "Number").Value) with _ -> 0
                                if (v.Element(xn "Number") <> null) then
                                    (int) (v.Element(xn "Number").Value) 
                                else 0 
                            yield number}
                  |> Seq.max

    let vars =
        seq { for v in vv do
                // watch out for null values! {Name,Range,...}
                let id = try (int)(v.Attribute(xn "Id").Value) with _ -> raise(MarshalInFailed(-1,"Bad Variable Id"))
                let name = try v.Element(xn "Name").Value with _ -> raise(MarshalInFailed(id,"Bad Name"))
                let min = try (int) (v.Element(xn "RangeFrom").Value) with _ -> raise(MarshalInFailed(id,"Bad RangeFrom"))
                let max = try (int) (v.Element(xn "RangeTo").Value) with _ -> raise(MarshalInFailed(id,"Bad RangeTo"))

               
                // Garvit's Shrink-Cut 
                // if no number given than generate a safe number
                let number = 
                    // try (int) (v.Element(xn "Number").Value) with _ -> mk_number_safe()
                    if (v.Element(xn "Number") <> null) then 
                        (int)(v.Element(xn "Number").Value) 
                    else mk_number_safe()
                let tt = 
                    // try v.Element(xn "Tags").Elements(xn "Tag") with _ -> Seq.empty
                    if (v.Element(xn "Tags") <> null && v.Element(xn "Tags").Elements(xn "Tag") <> null) then 
                        v.Element(xn "Tags").Elements(xn "Tag") 
                    else Seq.empty
                let tags = [for tag in tt do
                                    let tagId = try (int)(tag.Attribute(xn "Id").Value) with _ -> raise(MarshalInFailed(id, "Bad Tag Id"))
                                    let tagName = try (string)(tag.Attribute(xn "Name").Value) with _ -> raise(MarshalInFailed(id, "Bad Tag Name"))
                                    // this cell must have been declared in the cell names
                                    if tagName <> "_" && (not (List.exists (fun v -> v=tagName) ccNames)) then raise(MarshalInFailed(id, "No cell with that name"))
                                    yield (tagId, tagName)] 
                // by default each node is related to all cells
                let defaultTags = [for pos in 1 .. ccNames.Length do
                                        yield (pos, List.nth ccNames (pos-1))]
                // if no tags specified then replace with default tags
                let tags = if Seq.isEmpty tt then defaultTags else tags
                // Garvit


                // [t] can be None, in which case we'll synthesize a default T in [qn_map] later.
                let t = try Some ((string) (v.Element(xn "Function").Value)) with _ -> None
                let exn_msg f = "Failed to parse " + name + "'s transfer function" + f
                let parse_error f line col msg = 
                    "Failed to parse " + name + "'s function: " + f + ". " +
                    "Exception: " + msg + ". " +
                    "Will use default function."
                let fparsec t = 
                    match ParsecExpr.parse_expr t with
                    | ParsecExpr.ParseOK(f) -> Some f 
                    | ParsecExpr.ParseErr(err) -> 
                        Log.log_error(parse_error t err.line err.col err.msg) 
                        raise(MarshalInFailed(id,exn_msg t))
                let f =
                        match t with
                        | Some t when t="" -> None
                        | Some t -> fparsec t
                        | None -> None
                yield { Vid= id; Vname= name; Vfr= min; Vto= max; Vf= f; Vnumber=number; Vtags=tags } }

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
                    | None ->
                       // Changing default target function to a meaningful target function
                        match ii, oo with
                        | [], [] -> Expr.Const(min)
                        // If there are no positive influences and there are negative influences 
                        // 1. It should just be the constant min of range of the varialbe
                        // 2. The constituent value (with no inhibition) is the maximum. So the target function is max-ave(neg)
                        // 3. The target function is the general default function ave(pos)-ave(neg), where ave(pos) is replaced with
                        //    the min of the range of the variable as the list of pos is empty 
                        | [], _  -> Expr.Const(min) 
                                    // Expr.Minus(Expr.Const(max), Expr.Ave(List.map (fun o -> Expr.Var(o)) oo))
                                    // Expr.Max(Expr.Const(min), 
                                    //         Expr.Minus(Expr.Const(min),
                                    //                    Expr.Ave(List.map (fun o -> Expr.Var(o)) oo)))
                        | _ , [] -> Expr.Min(Expr.Const(max), Expr.Ave(List.map (fun i -> Expr.Var(i)) ii))
                                    // SI: Garvit's was:  Expr.Ave(List.map (fun i -> Expr.Var(i)) ii)
                        | _ , _  -> 
                                    Expr.Max(Expr.Const(min),
                                     Expr.Minus(Expr.Ave(List.map (fun i -> Expr.Var(i)) ii),
                                                Expr.Ave(List.map (fun o -> Expr.Var(o)) oo)))
                                    // SI: " Expr.Minus(Expr.Ave(List.map (fun i -> Expr.Var(i)) ii),Expr.Ave(List.map (fun o -> Expr.Var(o)) oo))
                let nature = 
                    // map describing the nature of inputs : activating/inhibiting
                    List.fold
                        (fun map i -> Map.add i QN.Act map)
                        (List.fold
                            (fun map o -> Map.add o QN.Inh map)
                            Map.empty
                            oo)
                        ii
                { QN.var= v.Vid; QN.range= (v.Vfr,v.Vto); QN.f= t; QN.inputs= ii@oo; QN.name= v.Vname; 
                    QN.nature= nature; QN.defaultF = Option.isNone v.Vf; QN.number = v.Vnumber; QN.tags = v.Vtags } )
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

let xml_of_smap (r:Map<var,int>) = 
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

    root.Add(new XElement(xn "Status", "Stabilizing"))
    List.iter (fun (t,bounds) -> add_a_tick root t bounds) []

    doc

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

let xml_of_ltl_string_result (result : bool) (model : string) =
    let doc = new XDocument()
    let root = new XElement(xn "AnalysisOutput")
    doc.AddFirst(root)

    match result with 
    | true -> 
        let status = new XElement(xn "Status", "True")
        root.Add(status)
    | false -> 
        let status = new XElement(xn "Status", "False")
        root.Add(status)
    
    let xml_model = new XElement(xn "Model", new XAttribute(xn "Id",model))
    root.Add(xml_model)

    doc

let xml_of_ltl_result_full (result:bool) (model:int * Map<int,Map<QN.var,int>>) = 
    let (loop,model_map) = model
    let doc = new XDocument()
    let root = new XElement(xn "AnalysisOutput")
    doc.AddFirst(root)
    
    match result with 
    | true -> 
        let status = new XElement(xn "Status", "True")
        root.Add(status)
    | false -> 
        let status = new XElement(xn "Status", "False")
        root.Add(status)
    
    let loopElem = new XElement(xn "Loop", loop)
    root.Add(loopElem)
    
    let i = ref 0
    while (Map.containsKey !i model_map) do

            let tick = new XElement(xn "Tick")
            let timeS = sprintf "%d" !i
            let time = new XElement(xn "Time", timeS)
            tick.Add(time)

            let variables = new XElement(xn "Variables")
            let map_var_to_value = Map.find !i model_map
            for entry in map_var_to_value do 
                let var = entry.Key
                let value = entry.Value
                let varXML = new XElement(xn "Variable")
                varXML.SetAttributeValue(xn "Id", var)
                varXML.SetAttributeValue(xn "Lo", value)
                varXML.SetAttributeValue(xn "Hi", value)
                variables.Add(varXML)
            
            tick.Add(variables)
            root.Add(tick)

            incr i
    doc

    
let xml_of_ltl_result (result:bool) (model:int * Map<int,Map<QN.var,int>>) = 
    let doc = new XDocument()
    let root = new XElement(xn "AnalysisOutput")
    doc.AddFirst(root)
    
    match result with 
    | true -> 
        let status = new XElement(xn "Status", "True")
        root.Add(status)
    | false -> 
        let status = new XElement(xn "Status", "False")
        root.Add(status)
    
    let model = new XElement(xn "Model", new XAttribute(xn "Id",model.ToString()))
    root.Add(model)
    
    doc

// Testing Synth's zipper. Function name, input, input
// let xml_of_synth_result (result:string) (model:int * Map<int,Map<QN.var,int>>) = 
// (model: QN.node list)
let xml_of_synth_result (result:string) (moreResult:string) (model:XDocument) = 
    let doc = new XDocument()
    let root = new XElement(xn "AnalysisOutput")
    doc.AddFirst(root)

    let output = new XElement(xn "Result", result)
    root.Add(output)

    let output2 = new XElement(xn "Details", moreResult)
    root.Add(output2)
    
    //let model = new XElement(xn "Model", new XAttribute(xn "Id",model.ToString()))
    let model = new XElement(xn "Model", model.ToString())
    root.Add(model)
    
    doc
// done


let xml_of_SCMResult (result:string) (details:string) = 
    let doc = new XDocument()
    let root = new XElement(xn "AnalysisOutput")
    doc.AddFirst(root)

    let output = new XElement(xn "Status", result)
    root.Add(output)

    let output2 = new XElement(xn "Details", details)
    root.Add(output2)
    
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


//
// SI: The json vs xml battle was fought: json won. 
//     

//
// Model to QN translation
//
let QN_of_Model (model:Model) = 
// SI: no Garvit stuff right now. Need to work out how to test for C# nulls.

// SI: no Garvit stuff right now. 
//    // Get cells
//    //let cc = try xd.Element(xn "AnalysisInput").Element(xn "Cells").Elements(xn "Cell") with _ -> Seq.empty
//    let cc = 
//        match model.Cells with 
//        | null -> Seq.empty
//        | cc -> Array.toSeq cc 
//            
//    let ccNames = if Seq.isEmpty cc then ["SingleCell"] 
//                  else [for cell in cc do yield try (string)cell.Name with _ -> raise(MarshalInFailed(-1, "Bad Cell Name"))]

    // Get vars
    //let vv = xd.Element(xn "AnalysisInput").Element(xn "Variables").Elements(xn "Variable")
    let vv = model.Variables

    // Get the max of all the variable ids to create safe ids for internal use
    // SI: this should be in QN
// SI: no Garvit stuff right now. 
//    let maxNumber_in_vv = Array.fold 
//                            (fun max_so_far (v:Model.Variable) -> 
//                                // SI: cast v.Number to see if it's non-null, don't know if that works? 
//                                let v = if v.Number.HasValue then v.Number.Value else max_so_far
//                                max v max_so_far) 
//                            0 
//                            vv 
//    maxNumber := max maxNumber_in_vv !maxNumber 

    let vars =
        seq { for v in vv do
                let id = v.Id
                let name = v.Name
                let min, max = (int)v.RangeFrom, (int)v.RangeTo

// SI: no Garvit stuff right now.                
//                // Garvit's Shrink-Cut 
//                // if no number given than generate a safe number
//                let number = 
//                    if v.Number.HasValue then v.Number.Value else mk_number_safe()
//                let tt = 
//                    match v.Tags with
//                    | null -> Seq.empty
//                    | tt -> Array.toSeq tt
//                let tags = [for tag in tt do
//                                    let tagId = tag.Id
//                                    let tagName = tag.Name
//                                    // this cell must have been declared in the cell names
//                                    if tagName <> "_" && (not (List.exists (fun v -> v=tagName) ccNames)) then raise(MarshalInFailed(id, "No cell with that name"))
//                                    yield (tagId, tagName)] 
//                // by default each node is related to all cells
//                let defaultTags = [for pos in 1 .. ccNames.Length do
//                                        yield (pos, List.nth ccNames (pos-1))]
//                // if no tags specified then replace with default tags
//                let tags = if Seq.isEmpty tt then defaultTags else tags
//                // Garvit
                let number = mk_number_safe ()
                let tags = []

                // [t] can be None, in which case we'll synthesize a default T in [qn_map] later.
                let t = Some v.Function
                let exn_msg f = "Failed to parse " + name + "'s transfer function" + f
                let parse_error f line col msg = 
                    "Failed to parse " + name + "'s function: " + f + ". " +
                    "Exception: " + msg + ". " +
                    "Will use default function."
                let fparsec t = 
                    match ParsecExpr.parse_expr t with
                    | ParsecExpr.ParseOK(f) -> Some f 
                    | ParsecExpr.ParseErr(err) -> 
                        Log.log_error(parse_error t err.line err.col err.msg) 
                        raise(MarshalInFailed(id,exn_msg t))
                let f =
                        match t with
                        | Some t when t="" -> None
                        | Some t -> fparsec t
                        | None -> None
                yield { Vid= id; Vname= name; Vfr= min; Vto= max; Vf= f; Vnumber=number; Vtags=tags } }

    let vars_map = Seq.fold (fun map (v:ui_variable) -> Map.add v.Vid v map) Map.empty vars

    // Get inputs
    let rr = model.Relationships
    let io = seq { for r in rr do
                    let id = r.Id
                    let source = r.FromVariableId
                    let target = r.ToVariableId
                    let parsed_rel = r.Type
                    let rel_ty =
                        match parsed_rel with
                        | Model.RelationshipType.Activator -> RTActivator
                        | Model.RelationshipType.Inhibitor -> RTInhibitor
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
                    | None ->
                       // Changing default target function to a meaningful target function
                        match ii, oo with
                        | [], [] -> Expr.Const(min)
                        // If there are no positive influences and there are negative influences 
                        // 1. It should just be the constant min of range of the varialbe
                        // 2. The constituent value (with no inhibition) is the maximum. So the target function is max-ave(neg)
                        // 3. The target function is the general default function ave(pos)-ave(neg), where ave(pos) is replaced with
                        //    the min of the range of the variable as the list of pos is empty 
                        | [], _  -> Expr.Const(min) 
                                    // Expr.Minus(Expr.Const(max), Expr.Ave(List.map (fun o -> Expr.Var(o)) oo))
                                    // Expr.Max(Expr.Const(min), 
                                    //         Expr.Minus(Expr.Const(min),
                                    //                    Expr.Ave(List.map (fun o -> Expr.Var(o)) oo)))
                        | _ , [] -> Expr.Min(Expr.Const(max), Expr.Ave(List.map (fun i -> Expr.Var(i)) ii))
                                    // SI: Garvit's was:  Expr.Ave(List.map (fun i -> Expr.Var(i)) ii)
                        | _ , _  -> 
                                    Expr.Max(Expr.Const(min),
                                     Expr.Minus(Expr.Ave(List.map (fun i -> Expr.Var(i)) ii),
                                                Expr.Ave(List.map (fun o -> Expr.Var(o)) oo)))
                                    // SI: " Expr.Minus(Expr.Ave(List.map (fun i -> Expr.Var(i)) ii),Expr.Ave(List.map (fun o -> Expr.Var(o)) oo))
                let nature = 
                    // map describing the nature of inputs : activating/inhibiting
                    List.fold
                        (fun map i -> Map.add i QN.Act map)
                        (List.fold
                            (fun map o -> Map.add o QN.Inh map)
                            Map.empty
                            oo)
                        ii
                { QN.var= v.Vid; QN.range= (v.Vfr,v.Vto); QN.f= t; QN.inputs= ii@oo; QN.name= v.Vname; 
                    QN.nature= nature; QN.defaultF = Option.isNone v.Vf; QN.number = v.Vnumber; QN.tags = v.Vtags } )
            inputs'

    // Final result
    let qn = Map.toList qn_map |> List.map (fun (_v,n) -> n)
    QN.qn_wf qn
    qn



// C# AnalysisResult = F# stability_result
let stability_result_of_AnalysisResult (ar:AnalysisResult) = 
    let parse_tick (tick:AnalysisResult.Tick) =
        let time = tick.Time
        let variables = tick.Variables
        let bounds =
            Seq.fold
                (fun (bb:QN.interval) (v:AnalysisResult.Tick.Variable) ->
                    let id = v.Id
                    let lo = (int)v.Lo
                    let hi = (int)v.Hi
                    Map.add id (lo,hi) bb)
                Map.empty
                variables
        (time,bounds)
    let status = ar.Status
    match status with
    | StatusType.Stabilizing ->
        let ticks = ar.Ticks
        let history = Seq.fold (fun h tick -> (parse_tick tick) :: h) [] ticks
        Result.SRStabilizing(history)
    | StatusType.NotStabilizing ->
        let ticks = ar.Ticks
        let history = Seq.fold (fun h tick -> (parse_tick tick) :: h) [] ticks
        Result.SRNotStabilizing(history)
    | x -> raise(MarshalInFailed(-1,"Bad status descriptor: "+x.ToString()))

let AnalysisResult_of_stability_result (sr:Result.stability_result) = 
    let Tick_of_tick (a:AnalysisResult.Tick[]) i (t:(int * QN.interval)) = 
        let (time,interval) = t
        a.[i] <- new AnalysisResult.Tick()
        a.[i].Time <- time
        a.[i].Variables <- 
            let vv = Array.zeroCreate (interval.Count)
            let vi = ref 0
            Map.iter 
                (fun v (lo,hi) -> 
                    let v' = new AnalysisResult.Tick.Variable ()
                    v'.Id <- v; v'.Lo <- (double)lo; v'.Hi <- (double)hi
                    vv.[!vi] <- v'
                    incr vi)
                interval
            vv
    let mk_AnalysisResult st err hist = 
        let ar = new AnalysisResult ()
        let ticks = Array.zeroCreate (List.length hist)
        List.iteri (Tick_of_tick ar.Ticks) hist
        ar.Status <- st
        ar.Error <- err
        ar.Ticks <- ticks 
        ar
    match sr with 
    | Result.SRStabilizing(hist) -> 
        mk_AnalysisResult StatusType.Stabilizing "" hist
    | Result.SRNotStabilizing(hist) -> 
        mk_AnalysisResult StatusType.NotStabilizing "" hist
        

let AnalysisResult_of_error id msg = 
    let r = new AnalysisResult()
    r.Status <- StatusType.Error
    r.Error <- (string)id + msg
    r

// C# CounterExampleOutput = F# cex_result
// stubs
let BifurcationCounterExample_of_CExBifurcation fix1 fix2 =
    assert(false)
    let cex = new BifurcationCounterExample()
    cex.Status <- CounterExampleType.Bifurcation
    cex.Error <- ""
    //cex.Variables
    // [A] X [B] -> [A X B]
    let vv = Array.zeroCreate (max (List.length fix1) (List.length fix2))

    cex.Variables <- vv
    cex 

let CycleCounterExample_of_CExCycle cyc = 
    assert(false)
    let cs_cex = new CycleCounterExample()
    cs_cex.Status <- CounterExampleType.Cycle
    cs_cex

let FixPointCounterExample_of_CExFixpoint fix = 
    assert(false)
    let cs_cex = new FixPointCounterExample()
    cs_cex.Status <- CounterExampleType.Fixpoint
    cs_cex



