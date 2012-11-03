(* Copyright (c) Microsoft Corporation. All rights reserved. *)
// Begin <-- Added by Qinsi Wang
// newest version of skin model
// still wait for phil's data

module currentSkinCell

// going to add shh signalling, p63, etc.

// wnt_target N notch_ic = N - notch_ic
//let wnt_target N notch_ic = Expr.Minus(Expr.Const N, Expr.Var notch_ic)
let wnt_target N p21 = Expr.Minus(Expr.Const N, Expr.Var p21)

// p21_target notch_ic = notch_ic
let p21_target notch_ic = Expr.Var notch_ic

// notch_ic_target notch_level deltaext = min(notch_level, deltaext)
let notch_ic_target notch_level deltaext = Expr.Min(Expr.Const notch_level, Expr.Var deltaext)

// deltaext_target deltas = ceil(average(deltas))
//let deltaext_target deltas = Expr.Ceil(Expr.Ave([for d in deltas -> Expr.Var d]))
let deltaext_target deltas = Expr.Sum([for d in deltas -> Expr.Var d])


// gt2_target notch_ic = notch_ic
let gt2_target notch_ic = Expr.Var notch_ic

// gt1_target bcat = bcat
let gt1_target bcat = Expr.Var bcat


// axin_target bcatexp axin = max(0, bcatexp - axin)
let axin_target cklalpha dsh = Expr.Max (Expr.Const 0, Expr.Minus(Expr.Var cklalpha, Expr.Var dsh))

// dsh_target frizzled = frizzled
let dsh_target frizzled = Expr.Var frizzled

// frizzled_target wntext = wntext
let frizzled_target wntext = Expr.Var wntext

// wntext_target wnts = floor(average(wnts))
//let wntext_target wnts = Expr.Floor(Expr.Ave([for w in wnts -> Expr.Var w]))
let wntext_target wnts = Expr.Sum([for w in wnts -> Expr.Var w])

// delta_target gt1 gt2 = ceil((gt1 + gt2)/2)
//let delta_target gt1 gt2 = Expr.Ceil(Expr.Ave [Expr.Var gt1; Expr.Var gt2])
let delta_target gt1 gt2 = Expr.Plus(Expr.Var gt1, Expr.Var gt2)

// bcatexp_target = 3
let bcatexp_target = Expr.Const 3

// cklalpha_target = 3
let cklalpha_target = Expr.Const 3

// const_target = C
let const_target C = Expr.Const C

// First try: add btrcp and a negative feedback loop involving btrcp and bcat
// does not work!! seems for all layers, bcat signalling is strongly downregulated!
// btrcp_target bcat = bcat
let btrcp_target bcat = Expr.Var bcat

// bcat_target bcatexp axin = max(0, bcatexp - axin)
let bcat_target bcatexp axin btrcp = Expr.Max (Expr.Const 0, Expr.Minus(Expr.Minus(Expr.Var bcatexp, Expr.Var axin), Expr.Var btrcp))

// Second try: 

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
    abstract member Bcatexp : QN.var
    abstract member Cklalpha : QN.var
    abstract member Frizzled : QN.var
    abstract member Dsh : QN.var
    abstract member Axin : QN.var
    abstract member Bcat : QN.var
    abstract member Gt1 : QN.var
    abstract member Gt2 : QN.var
    abstract member P21 : QN.var
    abstract member Btrcp : QN.var
    abstract member RegisterNeighbour : SkinCell -> unit
    abstract member Nodes : QN.node list

/// InternalSkinCell represents a physical cell in the skin.
type InternalSkinCell(N, notch, x, y, z) =
    let deltaext_id = QN.mk_var ()
    let notch_ic_id = QN.mk_var ()
    let wnt_id = QN.mk_var ()
    let wntext_id = QN.mk_var ()
    let delta_id = QN.mk_var ()
    let bcatexp_id = QN.mk_var ()
    let cklalpha_id = QN.mk_var ()
    let frizzled_id = QN.mk_var ()
    let dsh_id = QN.mk_var ()
    let axin_id = QN.mk_var ()
    let bcat_id = QN.mk_var ()
    let gt1_id = QN.mk_var ()
    let gt2_id = QN.mk_var ()
    let p21_id = QN.mk_var ()
    let btrcp_id = QN.mk_var ()
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

    let delta : QN.node = { var = delta_id; f = delta_target gt1_id gt2_id; inputs = [gt1_id; gt2_id];
                            name = "delta" + subscript  }
    let wnt : QN.node = { var = wnt_id; f =  wnt_target N p21_id; inputs = [p21_id];
                            name = "wnt" + subscript }
    let notch_ic : QN.node = { var = notch_ic_id; f = notch_ic_target notch deltaext_id; inputs = [deltaext_id];
                               name = "notchic" + subscript }
    let p21 : QN.node = { var = p21_id; f = p21_target notch_ic_id; inputs = [notch_ic_id];
                               name = "p21" + subscript }
    let gt1 : QN.node = { var = gt1_id; f = gt1_target bcat_id; inputs = [bcat_id];
                               name = "gt1" + subscript }
    let gt2 : QN.node = { var = gt2_id; f = gt2_target notch_ic_id; inputs = [notch_ic_id];
                               name = "gt2" + subscript }
    let bcat : QN.node = { var = bcat_id; f = bcat_target bcatexp_id axin_id btrcp_id; inputs = [bcatexp_id; axin_id; btrcp_id];
                               name = "bcat" + subscript }
    let axin : QN.node = { var = axin_id; f = axin_target cklalpha_id dsh_id; inputs = [cklalpha_id; dsh_id];
                               name = "axin" + subscript }
    let dsh : QN.node = { var = dsh_id; f = dsh_target frizzled_id; inputs = [frizzled_id];
                               name = "dsh" + subscript }
    let frizzled : QN.node = { var = frizzled_id; f = frizzled_target wntext_id; inputs = [wntext_id];
                               name = "frizzled" + subscript }
    let bcatexp : QN.node = { var = bcatexp_id; f = bcatexp_target; inputs = [];
                               name = "bcatexp" + subscript }
    let cklalpha : QN.node = { var = cklalpha_id; f = cklalpha_target; inputs = [];
                               name = "cklalpha" + subscript }
    let btrcp : QN.node = { var = btrcp_id; f = btrcp_target bcat_id; inputs = [bcat_id];
                               name = "btrcp" + subscript }
    // define both wntext, and deltaext later, because their new values need to be determined by other cells

    let mutable wntext_inputs = []
    let mutable deltaext_inputs = []

    interface SkinCell with
        member this.Delta = delta_id
        member this.Deltaext = deltaext_id
        member this.Wnt = wnt_id
        member this.Wntext = wntext_id
        member this.Notch_ic = notch_ic_id
        member this.Bcatexp = bcatexp_id
        member this.Cklalpha = cklalpha_id
        member this.Frizzled = frizzled_id
        member this.Dsh = dsh_id
        member this.Axin = axin_id
        member this.Bcat = bcat_id
        member this.Gt1 = gt1_id
        member this.Gt2 = gt2_id
        member this.P21 = p21_id
        member this.Btrcp = btrcp_id
        member this.RegisterNeighbour cell =
            wntext_inputs <- cell.Wnt :: wntext_inputs
            deltaext_inputs <- cell.Delta :: deltaext_inputs
        member this.Nodes =
            //printfn "I have %A neighbours" (List.length wntext_inputs)
            let wntext : QN.node = { var = wntext_id; f = wntext_target wntext_inputs; inputs = wntext_inputs;
                                     name = "wntext" + subscript }
            let deltaext : QN.node = { var = deltaext_id; f =  deltaext_target deltaext_inputs; inputs = deltaext_inputs;
                                       name = "deltaext" + subscript }
            [delta ; wnt ; notch_ic ; wntext ; deltaext; bcatexp; cklalpha; frizzled; dsh; axin; bcat; gt1; gt2; p21; btrcp]

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
        member this.Bcatexp = -1
        member this.Cklalpha = -1
        member this.Frizzled = -1
        member this.Dsh = -1
        member this.Axin = -1
        member this.Bcat = -1
        member this.Gt1 = -1
        member this.Gt2 = -1
        member this.P21 = -1
        member this.Btrcp = -1
        member this.RegisterNeighbour cell = ()
        member this.Nodes = [ wnt; delta ]
