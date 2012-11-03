(* Copyright (c) Microsoft Corporation. All rights reserved. *)
module Cycle3const

open Util


// v1_target v2 = v2
// There is no problem with ceil, plus, ave, var, const
//let v1_target v0 v2 = Expr.Plus(Expr.Ceil(Expr.Ave( [Expr.Var(v2); Expr.Var(v3)] )), Expr.Const(1))
let v1_target N v2 = Expr.Minus(Expr.Const(N), Expr.Var(v2))

// v2_target v3 = v3
let v2_target v3 = Expr.Var(v3)

// v3_target v1 = v1
let v3_target v1 = Expr.Var(v1)


// treat these three variables as within the same cell.
type CycleCell = 
    abstract member V1 : QN.var
    abstract member V2 : QN.var
    abstract member V3 : QN.var
    abstract member Nodes : QN.node list

type WholeCycleCell(N, x, y, z) = 
    let v1_id = QN.mk_var ()
    let v2_id = QN.mk_var ()
    let v3_id = QN.mk_var ()

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

    let v1 : QN.node = { var = v1_id; f = v1_target N v2_id; inputs = [v2_id];
                            name = "v1" + subscript  }
    let v2 : QN.node = { var = v2_id; f = v2_target v3_id; inputs = [v3_id];
                            name = "v2" + subscript  }
    let v3 : QN.node = { var = v3_id; f = v3_target v1_id; inputs = [v1_id];
                            name = "v3" + subscript  }

    interface CycleCell with
        member this.V1 = v1_id
        member this.V2 = v2_id
        member this.V3 = v3_id
        member this.Nodes = [v1; v2; v3]

let mk_column N y z =
    // These cells are actual physical skin cells.
    let cell = WholeCycleCell(N, Some(0), y, z) :> CycleCell
    // Return the
    [cell]


let mk_Cycle3 N =
    let column = mk_column N None None
    List.fold (fun l (cell : CycleCell) -> l @ cell.Nodes) [] column