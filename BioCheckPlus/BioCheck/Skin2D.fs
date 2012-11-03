(* Copyright (c) Microsoft Corporation. All rights reserved. *)
module Skin2D

open SkinCell
open Util


/// A 2D model of a cross section of skin.

// Wire up two adjacent columns of cells by connecting adjacent pairs of cells.
let wire_columns (col1 : SkinCell list) col2 =
    for (x, y) in List.zip col1 col2 do
        x.RegisterNeighbour(y)
        y.RegisterNeighbour(x)


// Build a 2D cross section of skin.
// As with Skin1D, dummy1 is the "dermis", dummy2 is the "corneum" and N is the model granularity.
// width is the side length of the cross section, so for example,
//  mk_cross_section d1 d2 10 N
// produces a 10 column long cross section of cells.
let mk_cross_section dummy1 dummy2 width N z =
    // just construct valueof(width) columns internal cells
    // also name the cells also with y axis info (Sone(i-1))
    let columns = [for i in [1..width] -> Skin1D.mk_column dummy1 dummy2 N (Some(i-1)) z]

    // Wire up cells in adjacent columns.
    for (col1, col2) in (List.zip (take (width-1) columns) (List.tail columns)) do
        wire_columns col1 col2

    // Return the cross section.
    columns


// Construct a QN network modelling a 2D cross section of skin cells, where
// each QN variable can take values in the range [0..N].
let mk_Skin2D N width =
    // The dummy cells represent the space below the skin and above the skin respectively.
//    let dummy1 = DummySkinCell(N, (N + 1) / 2) :> SkinCell
//    let dummy2 = DummySkinCell(0, (N + 1) / 2) :> SkinCell
    let dummy1 = DummySkinCell(N, (N + 1) / 2, Some(-1), Some(-1), None) :> SkinCell
    let dummy2 = DummySkinCell(0, (N + 1) / 2, Some(10), Some(10), None) :> SkinCell

    let section = mk_cross_section dummy1 dummy2 width N None

    // Pull all the cells out of the model...
    let skin_cells = List.fold (fun column cells -> column @ cells) [] section
    let cells = dummy1 :: dummy2 :: skin_cells

    // Now create a model by pulling out all of the QN nodes in each cell.
    List.fold (fun l (cell : SkinCell) -> l @ cell.Nodes) [] cells
