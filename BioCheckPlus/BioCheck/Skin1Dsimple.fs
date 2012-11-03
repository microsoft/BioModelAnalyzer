(* Copyright (c) Microsoft Corporation. All rights reserved. *)
module Skin1Dsimple

open SkinCellchange //stable
//open nuSkinCell // unstable
//open currentSkinCell
open Util

/// A 1D model of the skin, constructed from 5 skin cells.


// Build a column of skin cells.
// dummy1 represents the cells below this column ("dermis" in the Java code)
// dummy2 represents the cells above this  column ("corneum" in the Java code)
// N is the granularity of the model.
let mk_column dummy1 dummy2 N y z =
    // These cells are actual physical skin cells.
    // Buggy one notch 0,2,3,3,3
    // fixed one notch 1,2,3,3,3
    //let cell1 = InternalSkinCell(N, 0, Some(0), y, z) :> SkinCell
    let cell1 = InternalSkinCell(N, 1, Some(0), y, z) :> SkinCell
    let cell2 = InternalSkinCell(N, 2, Some(1), y, z) :> SkinCell
    let cell3 = InternalSkinCell(N, 3, Some(2), y, z) :> SkinCell
    let cell4 = InternalSkinCell(N, 3, Some(3), y, z) :> SkinCell
    let cell5 = InternalSkinCell(N, 3, Some(4), y, z) :> SkinCell

    let internal_cells = [cell1; cell2; cell3; cell4; cell5]
    //let internal_cells = [cell1]
    //let cells = dummy1 :: internal_cells @ [dummy2]
    let cells = internal_cells
    let ncells = List.length cells

    // Wire up the neighbouring cells to each other...
    for (x, y) in List.zip (take (ncells-1) cells) (List.tail cells) do
        x.RegisterNeighbour(y)
        y.RegisterNeighbour(x)

    // Return the
    internal_cells


// Construct a QN network modelling a 1D column of skin cells, where
// each QN variable can take values in the range [0..N]
let mk_Skin1D N =
    // The dummy cells represent the space below the skin and above the skin respectively.
    let dummy1 = DummySkinCell(N, (N + 1) / 2, Some(-1), None, None) :> SkinCell
    let dummy2 = DummySkinCell(0, (N + 1) / 2, Some(5), None, None) :> SkinCell

    let column = mk_column dummy1 dummy2 N None None
    let full_column = dummy1 :: dummy2 :: column // why not "dummy1 :: column :: dummy2"? It does matter,right?
    //let full_column = dummy1 :: column @ [dummy2]
    //let full_column = column

    // Now create a model by pulling out all of the QN nodes in each cell.
    List.fold (fun l (cell : SkinCell) -> l @ cell.Nodes) [] full_column