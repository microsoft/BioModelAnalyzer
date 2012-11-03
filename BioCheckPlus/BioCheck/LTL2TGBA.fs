(* Copyright (c) Microsoft Corporation. All rights reserved. *)

//Begin <-- Added by Qinsi Wang
// This module is used to read in the resulting automaton
// and parse it in a proper way. (To be extended then)

module LTL2TGBA

open System.IO

let readLines filePath = System.IO.File.ReadLines(filePath)