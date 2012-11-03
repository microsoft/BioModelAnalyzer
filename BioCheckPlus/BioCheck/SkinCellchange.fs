(* Copyright (c) Microsoft Corporation. All rights reserved. *)
module SkinCellchange
//the SSkinFxd/ SSkin models

// define the target functions for all components within a cell - wnt, wntext, notchic, deltaext, delta, and other constant inputs.
// describe the simple model on Page 5, given by Fug. 1.

// wnt_target N notch_ic = N - notch_ic
let wnt_target N notch_ic = Expr.Minus(Expr.Const N, Expr.Var notch_ic)

// wntext_target wnts = floor(average(wnts))
//let wntext_target wnts = Expr.Floor(Expr.Ave([for w in wnts -> Expr.Var w]))
let wntext_target wnts = Expr.Sum([for w in wnts -> Expr.Var w])

// notch_ic_target notch_level deltaext = min(notch_level, deltaext)
let notch_ic_target notch_level deltaext = Expr.Min(Expr.Const notch_level, Expr.Var deltaext)

// deltaext_target deltas = ceil(average(deltas))
//let deltaext_target deltas = Expr.Ceil(Expr.Ave([for d in deltas -> Expr.Var d]))
//let deltaext_target deltas = Expr.Floor(Expr.Ave([for d in deltas -> Expr.Var d]))
let deltaext_target deltas = Expr.Sum([for d in deltas -> Expr.Var d])

// delta_target wntext notch_ic = ceil((wntext + notch_ic) / 2)
//let delta_target wntext notch_ic = Expr.Ceil(Expr.Div(Expr.Plus(Expr.Var wntext, Expr.Var notch_ic), Expr.Const 2))
let delta_target wntext notch_ic = Expr.Ceil(Expr.Ave [Expr.Var wntext; Expr.Var notch_ic])
//let delta_target wntext notch_ic = Expr.Floor(Expr.Ave [Expr.Var wntext; Expr.Var notch_ic])


// const_target = C
let const_target C = Expr.Const C

/// SkinCell represents a single skin cell.
/// This class acts as a factory for QN nodes.  First, you create all the SkinCells you need in your model,
/// then you tell each cell which other cells are its neighbours (by calling RegisterNeighbour), then you
/// ask the cell for all the QN nodes it contains (by calling Nodes).  The resulting list of nodes are all
/// connected appropriately.
type SkinCell =
    abstract member Delta : QN.var
    abstract member Deltaext : QN.var
    abstract member Wnt : QN.var
    abstract member Wntext : QN.var
    abstract member Notch_ic : QN.var
    abstract member RegisterNeighbour : SkinCell -> unit
    abstract member Nodes : QN.node list

/// InternalSkinCell represents a physical cell in the skin.
type InternalSkinCell(N, notch, x, y, z) =
    let deltaext_id = QN.mk_var ()
    let notch_ic_id = QN.mk_var ()
    let wnt_id = QN.mk_var ()
    let wntext_id = QN.mk_var ()
    let delta_id = QN.mk_var ()
    //let notch_id = QN.mk_var () //since notch has constant value here

    let subscript =
        match x with
        | None -> ""
        | Some(i: int) -> string(i)
        +
        match y with
        | None -> ""
        | Some(j: int) -> "_" + string(j)
        +
        match z with
        | None -> ""
        | Some(k: int) -> "_" + string(k)

    let delta : QN.node = { var = delta_id; f = delta_target wntext_id notch_ic_id; inputs = [wntext_id; notch_ic_id];
                            name = "delta" + subscript  }
    let wnt : QN.node = { var = wnt_id; f =  wnt_target N notch_ic_id; inputs = [notch_ic_id];
                            name = "wnt" + subscript }
    let notch_ic : QN.node = { var = notch_ic_id; f = notch_ic_target notch deltaext_id; inputs = [deltaext_id];
                               name = "notchic" + subscript }
    // define both wntext, and deltaext later, because their new values need to be determined by other cells

    let mutable wntext_inputs = []
    let mutable deltaext_inputs = []

    interface SkinCell with
        member this.Delta = delta_id
        member this.Deltaext = deltaext_id
        member this.Wnt = wnt_id
        member this.Wntext = wntext_id
        member this.Notch_ic = notch_ic_id
        member this.RegisterNeighbour cell =
            wntext_inputs <- cell.Wnt :: wntext_inputs
            deltaext_inputs <- cell.Delta :: deltaext_inputs
        member this.Nodes =
            //printfn "I have %A neighbours" (List.length wntext_inputs)
            let wntext : QN.node = { var = wntext_id; f = wntext_target wntext_inputs; inputs = wntext_inputs;
                                     name = "wntext" + subscript }
            let deltaext : QN.node = { var = deltaext_id; f =  deltaext_target deltaext_inputs; inputs = deltaext_inputs;
                                       name = "deltaext" + subscript }
            [delta ; wnt ; notch_ic ; wntext ; deltaext]

/// DummySkinCell represents a boundary - it models the environment the skin lives in by providing constant
/// levels of delta and wnt.
type DummySkinCell(wnt_level, delta_level, x, y, z) =
    let wnt_id = QN.mk_var ()
    let delta_id = QN.mk_var ()

    let subscript =
        match x with
        | None -> ""
        | Some(i: int) -> string(i)
        +
        match y with
        | None -> ""
        | Some(j: int) -> "_" + string(j)
        +
        match z with
        | None -> ""
        | Some(k: int) -> "_" + string(k)
    
    // When unrolling the system, subscripts are 

    let wnt : QN.node = { var = wnt_id; f = const_target wnt_level; inputs = [];
                          name = "dummy_wnt" + subscript}
    let delta : QN.node = { var = delta_id; f = const_target delta_level; inputs = [];
                            name = "dummy_delta" + subscript}

    interface SkinCell with
        member this.Delta = delta_id
        member this.Wnt = wnt_id
        member this.Deltaext = -1
        member this.Wntext = -1
        member this.Notch_ic = -1
        member this.RegisterNeighbour cell = ()
        member this.Nodes = [ wnt; delta ]
