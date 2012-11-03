(* Copyright (c) Microsoft Corporation. All rights reserved. *)
module Skin3Dsimple

open SkinCellchange
//open nuSkinCell
//open currentSkinCell
open Util


/// A 3D model of a region of skin.


// Build a 2D cross section of skin.
// As with Skin1D, dummy1 is the "dermis", dummy2 is the "corneum" and N is the model granularity.
// width is the side length of the cross section, so for example,
//  mk_mesh d1 d2 10 N
// produces a 10x10 mesh of cells.
let mk_mesh dummy1 dummy2 width N =
    let sections = [for i in [1..width] -> Skin2Dsimple.mk_cross_section dummy1 dummy2 width N (Some(i-1))]

    // Wire up cells in adjacent cross sections.
    for (sec1, sec2) in (List.zip (take (width-1) sections) (List.tail sections)) do
        for (col1, col2) in List.zip sec1 sec2 do
            Skin2Dsimple.wire_columns col1 col2

    // Return the cross section.
    sections


// Construct a QN network modelling a 2D cross section of skin cells, where
// each QN variable can take values in the range [0..N].
let mk_Skin3D N width =
    // The dummy cells represent the space below the skin and above the skin respectively.
    let dummy1 = DummySkinCell(N, (N + 1) / 2, Some(-1), Some(-1), Some(-1)) :> SkinCell
    let dummy2 = DummySkinCell(0, (N + 1) / 2, Some(20), Some(20), Some(20)) :> SkinCell

    let mesh = mk_mesh dummy1 dummy2 width N

    // Pull all the cells out of the model...
    let columns = List.fold (fun section columns -> section @ columns) [] mesh
    let skin_cells = List.fold (fun column cells -> column @ cells) [] columns
    let cells = dummy1 :: dummy2 :: skin_cells

    // Now create a model by pulling out all of the QN nodes in each cell.
    List.fold (fun l (cell : SkinCell) -> l @ cell.Nodes) [] cells