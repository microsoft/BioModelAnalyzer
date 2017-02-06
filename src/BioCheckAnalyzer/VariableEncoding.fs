// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE

module VariableEncoding

open Microsoft.Z3

let gensym =
    let counter = ref 0
    (fun (s : string) -> incr counter; s + ((string)!counter))

let enc_z3_int_var_at_time (node : QN.node) time = sprintf "v%d^%d" node.var time

let enc_z3_bool_var_at_time_in_val_from_var variable time value = sprintf "v%d^%d^%d" variable time value
let enc_z3_bool_var_at_time_in_val (node : QN.node) time value =  enc_z3_bool_var_at_time_in_val_from_var node.var time value

let enc_z3_bool_var_formula_in_location_at_time (location : int list) time =
    let f_with_location = List.fold (fun name value -> sprintf "%s^%d" name value) "f" location
    sprintf "%s^^%d" f_with_location time
let enc_z3_bool_var_loop_at_time time = sprintf "l^%d" time
let enc_z3_bool_var_trans_of_var_from_time_to_time_uniqueid (node : QN.node) from_time to_time value = sprintf "tv%d^%d^%d^%s" node.var from_time to_time value


let make_z3_int_var (name : string) (z : Context) = z.MkIntConst(z.MkSymbol(name))
let make_z3_bool_var (name : string) (z : Context) = z.MkBoolConst(z.MkSymbol(name))

let dec_qn_var_from_z3_var (name : string) =
    let parts = name.Split[|'^'|]
    let id = (parts.[0]).Substring 1
    ((int id) : QN.var)

let dec_qn_var_at_t_from_z3_var (name : string) =
    let parts = name.Split[|'^'|]
    let id = (parts.[0]).Substring 1
    ((int id),(int parts.[1]) : QN.var * int)


//
//
// ============================================================================
// The env encoding is different as it does not include the v before the %d^%d!
// ============================================================================
//
//
let enc_for_env_qn_id_string_at_t (id : string) time =
    (id +  "^" + ((string)time))
let enc_env_qn_id_string_at_t (id : string) time =
    (id +  "^" + ((string)time))

let enc_for_env_qn_id_at_t (id : QN.var) time =
    enc_for_env_qn_id_string_at_t ((string) id) time
let enc_env_qn_id_at_t (id : QN.var) time =
    enc_env_qn_id_string_at_t ((string) id) time

let dec_from_env_qn_id_at_t (name : string) = 
    let parts = name.Split[|'^'|]
    let id = parts.[0]
    ((id),((int)parts.[1]))
let dec_id_at_t_from_env (name : string) = 
    let parts = name.Split[|'^'|]
    let id = parts.[0]
    ((id),((int)parts.[1]))



