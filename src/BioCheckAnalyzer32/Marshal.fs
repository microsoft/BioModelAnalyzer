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

//
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
                let t = Some v.Formula
                let exn_msg f = "Failed to parse " + name + "'s transfer function " + f
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
                    let source = r.FromVariable
                    let target = r.ToVariable
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
                    v'.Id <- v; v'.Lo <- lo; v'.Hi <- hi
                    vv.[!vi] <- v'
                    incr vi)
                interval
            vv
    let mk_AnalysisResult st err hist = 
        let ar = new AnalysisResult ()
        let ticks = Array.zeroCreate (List.length hist)
        List.iteri (Tick_of_tick ticks) hist
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
let BifurcationCounterExample_of_CExBifurcation (fix1:Map<string, int>) (fix2:Map<string, int>) =
    assert(fix1.Count = fix2.Count)
    let cex = new BifurcationCounterExample()
    cex.Status <- CounterExampleType.Bifurcation
    cex.Error <- ""
    //cex.Variables
    // [A] X [B] -> [A X B]
    let len = fix1.Count
    let vv = Array.zeroCreate (len)
    let vv_idx = ref 0
    Map.iter
        (fun k v ->
            let bv =  new BifurcationCounterExample.BifurcatingVariable()
            bv.Id <- k 
            bv.Fix1 <- v 
            bv.Fix2 <- Map.find k fix2
            vv.[!vv_idx] <- bv
            incr vv_idx)
        fix1 
    cex.Variables <- vv
    cex 

let CycleCounterExample_of_CExCycle (cyc:Map<string, int>) = 
    let cex = new CycleCounterExample()
    cex.Status <- CounterExampleType.Cycle
    cex.Error <- ""
    let vv = Array.zeroCreate (cyc.Count)
    let vv_idx = ref 0
    Map.iter 
        (fun k v ->
            let cv = new CounterExampleOutput.Variable ()
            cv.Id <- k
            cv.Value <- v
            vv.[!vv_idx] <- cv
            incr vv_idx)
        cyc
    cex.Variables <- vv
    cex

let FixPointCounterExample_of_CExFixpoint (fix:Map<string, int>) = 
    let cex = new FixPointCounterExample()
    cex.Status <- CounterExampleType.Fixpoint
    cex.Error <- ""
    let vv = Array.zeroCreate (fix.Count)
    let vv_idx = ref 0
    Map.iter 
        (fun k v ->
            let cv = new CounterExampleOutput.Variable ()
            cv.Id <- k
            cv.Value <- v
            vv.[!vv_idx] <- cv
            incr vv_idx)
        fix
    cex.Variables <- vv
    cex

let CounterExampleOutput_of_cex_result cr = 
    match cr with 
    | Result.CExBifurcation(fix1,fix2) -> 
        (BifurcationCounterExample_of_CExBifurcation fix1 fix2)  :> CounterExampleOutput
    | Result.CExCycle(cyc) -> 
        (CycleCounterExample_of_CExCycle cyc) :> CounterExampleOutput
    | Result.CExFixpoint(fix) -> 
        (FixPointCounterExample_of_CExFixpoint fix) :> CounterExampleOutput
    | Result.CExUnknown -> 
        let cex = new CounterExampleOutput()
        cex.Status <- CounterExampleType.Unknown
        cex.Error <- "CExUnknown"
        cex


