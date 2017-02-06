// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
module Models

open Simulator
open Automata

let test_automata () = 
    let rely = new Automata.SimpleAutomata<int, interp>()
    rely.addInitialState(0);
    rely.addState(0, Map.empty)
    rely.addEdge(0,0)
    sim 
        (context.MkEq(context.MkIntConst("next_x"), context.MkInt(0)))
        (context.MkOr(
            context.MkEq(context.MkIntConst("next_x"), context.MkInt(2)),
            context.MkEq(context.MkIntConst("next_x"), context.MkInt(1))
        ))  
        rely  

let test_automata2 () = 
    let rely = new Automata.SimpleAutomata<int, interp>()
    rely.addInitialState(0);
    rely.addState(0, Map.empty)
    rely.addEdge(0,0)
    sim 
        (context.MkEq(context.MkIntConst("next_x"), context.MkInt(0)))
        (context.MkAnd(
            context.MkImplies(
                context.MkLt(context.MkIntConst("prev_x"), context.MkInt(4)),
                context.MkEq(context.MkIntConst("next_x"), context.MkAdd(context.MkIntConst("prev_x"), context.MkInt(1)))
            ),
            context.MkImplies(
                context.MkEq(context.MkIntConst("prev_x"), context.MkInt(4)),
                context.MkEq(context.MkIntConst("next_x"), context.MkInt(1))
            )
        ))  
        rely  

///Test automata fro 2008 paper, with floating inputs
let test_automata3<'istate when 'istate : comparison> : Automata<'istate,interp> -> SimpleAutomata<int, interp * ('istate option)> =     

    let bound f l h =  ((var f << int h) &&& ((int l << var f) ||| (var f === int l)))
    sim 
        (bound "next_path" 1 4 &&& bound "next_signal" 1 4)
        (context.MkAnd 
            [|
            (* Straight from the paper *)
            (* Path rules *)
            ((var "prev_path" << int 4) &&& (int 0 << var "prev_path") &&& (var "next_signal" << int 4) &&& (var "prev_input" === int 0))
                ==> (((var "prev_path") ++ (int 1)) === (var "next_path"))

            ((var "prev_path" << int 4) &&& (int 0 << var "prev_path") &&& (var "next_signal" << int 4) &&& (var "prev_input" === int 1))
                ==> (int 4 === (var "next_path"))

            ((var "prev_path" << int 4) &&& (int 0 << var "prev_path") &&& (var "next_signal" === int 4))
                ==> (int 0 === (var "next_path"))
            (* Signal rules *)
            ((var "prev_neighbour_path" === int 4)  &&& (int 0 << var "prev_signal"))
                ==> (var "next_signal" === int 4)

            ((var "prev_neighbour_path" << int 4)  &&& (int 4 === var "prev_path"))
                ==> (var "next_signal" === int 0)

            ((var "prev_neighbour_path" << int 4)  &&& (var "prev_path" << int 4) &&& (int 0 << var "prev_signal") &&& (var "prev_signal" << int 4))
                ==> ((var "next_signal" ++ int 1) === var "prev_signal")

            (* Rules for dealing with not performing an update*)
            ((var "prev_neighbour_path" << int 4)  &&& (var "prev_path" << int 4) &&& (var "prev_signal" === int 4))
                ==> (int 4 === var "prev_signal")

            ((var "prev_signal" === int 0)  ==> (var "next_signal" === int 0))

            ((var "prev_path" === int 0) ||| (var "prev_path" === int 4)) 
                ==> (var "prev_path" === var "next_path")

            ((var "prev_path" << int 5) &&& ((int 0 << var "prev_path") ||| (var "prev_path" === int 0)))

            bound "prev_path" 0 5

            bound "next_path" 0 5

            bound "prev_signal" 0 5

            bound "next_signal" 0 5
            |]
        )  
        
///Test automata fro 2008 paper, with Constrained inputs
let test_automata4<'istate when 'istate : comparison> : Automata<'istate,interp> -> SimpleAutomata<int, interp * ('istate option)> =     
    sim 
        (bound "next_path" 1 2 &&& bound "next_signal" 3 4)
        (context.MkAnd 
            [|
            (* Straight from the paper *)
            (* Path rules *)
            ((var "prev_path" << int 4) &&& (int 0 << var "prev_path") &&& (var "next_signal" << int 4) &&& (var "prev_input" === int 0))
                ==> (((var "prev_path") ++ (int 1)) === (var "next_path"))

            ((var "prev_path" << int 4) &&& (int 0 << var "prev_path") &&& (var "next_signal" << int 4) &&& (var "prev_input" === int 1))
                ==> (int 4 === (var "next_path"))

            ((var "prev_path" << int 4) &&& (int 0 << var "prev_path") &&& (var "next_signal" === int 4))
                ==> (int 0 === (var "next_path"))
            (* Signal rules *)
            ((var "prev_neighbour_path" === int 4)  &&& (int 0 << var "prev_signal"))
                ==> (var "next_signal" === int 4)

            ((var "prev_neighbour_path" << int 4)  &&& (int 4 === var "prev_path"))
                ==> (var "next_signal" === int 0)

            ((var "prev_neighbour_path" << int 4)  &&& (var "prev_path" << int 4) &&& (int 0 << var "prev_signal") &&& (var "prev_signal" << int 4))
                ==> ((var "next_signal" ++ int 1) === var "prev_signal")

            (* Rules for dealing with not performing an update*)
            ((var "prev_neighbour_path" << int 4)  &&& (var "prev_path" << int 4) &&& (var "prev_signal" === int 4))
                ==> (int 4 === var "prev_signal")

            ((var "prev_signal" === int 0)  ==> (var "next_signal" === int 0))

            ((var "prev_path" === int 0) ||| (var "prev_path" === int 4)) 
                ==> (var "prev_path" === var "next_path")

            ((var "prev_path" << int 5) &&& ((int 0 << var "prev_path") ||| (var "prev_path" === int 0)))

            bound "prev_path" 0 5

            bound "next_path" 0 5

            bound "prev_signal" 0 5

            bound "next_signal" 0 5
            |]
        )  
        



///Test automata fro 2008 paper, with Constrained inputs
///Has separate left and right paths
let test_automata5<'istate when 'istate : comparison> : Automata<'istate,interp> -> SimpleAutomata<int, interp * ('istate option)> =     
    sim 
        (bound "next_path" 1 2 &&& bound "next_signal" 3 4)
        (context.MkAnd 
            [|
            (* Straight from the paper *)
            (* Path rules *)
            ((var "prev_path" << int 4) &&& (int 0 << var "prev_path") &&& (var "next_signal" << int 4) &&& (var "prev_input" === int 0))
                ==> (((var "prev_path") ++ (int 1)) === (var "next_path"))

            ((var "prev_path" << int 4) &&& (int 0 << var "prev_path") &&& (var "next_signal" << int 4) &&& (var "prev_input" === int 1))
                ==> (int 4 === (var "next_path"))

            ((var "prev_path" << int 4) &&& (int 0 << var "prev_path") &&& (var "next_signal" === int 4))
                ==> (int 0 === (var "next_path"))
            (* Signal rules *)
            (((var "prev_left_path" === int 4) ||| (var "prev_right_path" === int 4)) &&& (int 0 << var "prev_signal"))
                ==> (var "next_signal" === int 4)

            ((var "prev_left_path" << int 4)  &&& (var "prev_right_path" << int 4)  &&& (int 4 === var "prev_path"))
                ==> (var "next_signal" === int 0)

            ((var "prev_left_path" << int 4)  &&& (var "prev_right_path" << int 4)  &&& (var "prev_path" << int 4) &&& (int 0 << var "prev_signal") &&& (var "prev_signal" << int 4))
                ==> ((var "next_signal" ++ int 1) === var "prev_signal")

            (* Rules for dealing with not performing an update*)
            ((var "prev_left_path" << int 4)  &&& (var "prev_right_path" << int 4)  &&& (var "prev_path" << int 4) &&& (var "prev_signal" === int 4))
                ==> (int 4 === var "prev_signal")

            ((var "prev_signal" === int 0)  ==> (var "next_signal" === int 0))

            ((var "prev_path" === int 0) ||| (var "prev_path" === int 4)) 
                ==> (var "prev_path" === var "next_path")

            ((var "prev_path" << int 5) &&& ((int 0 << var "prev_path") ||| (var "prev_path" === int 0)))

            bound "prev_path" 0 5

            bound "next_path" 0 5

            bound "prev_signal" 0 5

            bound "next_signal" 0 5
            |]
        )  

let test_automata5_fate_clock = 
        (bound "next_path" 1 2 &&& bound "next_signal" 3 4 &&& bound "next_fate" 0 1 &&& bound "next_clock" 0 1),
        (context.MkAnd 
            [|
            (* Straight from the paper *)
            (* Path rules *)
            ((var "prev_path" << int 4) &&& (int 0 << var "prev_path") &&& (var "next_signal" << int 4) &&& (var "prev_input" === int 0))
                ==> (((var "prev_path") ++ (int 1)) === (var "next_path"))

            ((var "prev_path" << int 4) &&& (int 0 << var "prev_path") &&& (var "next_signal" << int 4) &&& (var "prev_input" === int 1))
                ==> (int 4 === (var "next_path"))

            ((var "prev_path" << int 4) &&& (int 0 << var "prev_path") &&& (var "next_signal" === int 4))
                ==> (int 0 === (var "next_path"))
            (* Signal rules *)
            (((var "prev_left_path" === int 4) ||| (var "prev_right_path" === int 4)) &&& (int 0 << var "prev_signal"))
                ==> (var "next_signal" === int 4)

            ((var "prev_left_path" << int 4)  &&& (var "prev_right_path" << int 4)  &&& (int 4 === var "prev_path"))
                ==> (var "next_signal" === int 0)

            ((var "prev_left_path" << int 4)  &&& (var "prev_right_path" << int 4)  &&& (var "prev_path" << int 4) &&& (int 0 << var "prev_signal") &&& (var "prev_signal" << int 4))
                ==> ((var "next_signal" ++ int 1) === var "prev_signal")

            (* Rules for dealing with not performing an update*)
            ((var "prev_left_path" << int 4)  &&& (var "prev_right_path" << int 4)  &&& (var "prev_path" << int 4) &&& (var "prev_signal" === int 4))
                ==> (int 4 === var "prev_signal")

            ((var "prev_signal" === int 0)  ==> (var "next_signal" === int 0))

            ((var "prev_path" === int 0) ||| (var "prev_path" === int 4)) 
                ==> (var "prev_path" === var "next_path")

            ((var "prev_path" << int 5) &&& ((int 0 << var "prev_path") ||| (var "prev_path" === int 0)))

            (*clock updates*)

            var "prev_clock" << int 10 ==> (var "next_clock" === var "prev_clock" ++ int 1)

            var "prev_clock" === int 10 ==> (var "next_clock" === var "prev_clock")

            (*fate updates*)

            var "next_clock" << int 10 ==> (var "next_fate" === int 0)

            (var "next_clock" === int 10) &&& (var "next_path" === int 4) ==> (var "next_fate" === int 1)

            (var "next_clock" === int 10) &&& (var "next_path" === int 0) ==> (var "next_fate" === int 2)

            (var "next_clock" === int 10) &&& (int 0 << var "next_path") &&& (var "next_path" << int 4) ==> (var "next_fate" === int 0)

            bound "prev_path" 0 5

            bound "next_path" 0 5

            bound "prev_signal" 0 5

            bound "next_signal" 0 5

            bound "next_fate" 0 3

            bound "next_clock" 0 11
            |]
        )  

let test_automata5_forms = 
        (bound "next_path" 1 2 &&& bound "next_signal" 3 4),
        (context.MkAnd 
            [|
            (* Straight from the paper *)
            (* Path rules *)
            ((var "prev_path" << int 4) &&& (int 0 << var "prev_path") &&& (var "next_signal" << int 4) &&& (var "prev_input" === int 0))
                ==> (((var "prev_path") ++ (int 1)) === (var "next_path"))

            ((var "prev_path" << int 4) &&& (int 0 << var "prev_path") &&& (var "next_signal" << int 4) &&& (var "prev_input" === int 1))
                ==> (int 4 === (var "next_path"))

            ((var "prev_path" << int 4) &&& (int 0 << var "prev_path") &&& (var "next_signal" === int 4))
                ==> (int 0 === (var "next_path"))
            (* Signal rules *)
            (((var "prev_left_path" === int 4) ||| (var "prev_right_path" === int 4)) &&& (int 0 << var "prev_signal"))
                ==> (var "next_signal" === int 4)

            ((var "prev_left_path" << int 4)  &&& (var "prev_right_path" << int 4)  &&& (int 4 === var "prev_path"))
                ==> (var "next_signal" === int 0)

            ((var "prev_left_path" << int 4)  &&& (var "prev_right_path" << int 4)  &&& (var "prev_path" << int 4) &&& (int 0 << var "prev_signal") &&& (var "prev_signal" << int 4))
                ==> ((var "next_signal" ++ int 1) === var "prev_signal")

            (* Rules for dealing with not performing an update*)
            ((var "prev_left_path" << int 4)  &&& (var "prev_right_path" << int 4)  &&& (var "prev_path" << int 4) &&& (var "prev_signal" === int 4))
                ==> (int 4 === var "prev_signal")

            ((var "prev_signal" === int 0)  ==> (var "next_signal" === int 0))

            ((var "prev_path" === int 0) ||| (var "prev_path" === int 4)) 
                ==> (var "prev_path" === var "next_path")

            ((var "prev_path" << int 5) &&& ((int 0 << var "prev_path") ||| (var "prev_path" === int 0)))

            bound "prev_path" 0 5

            bound "next_path" 0 5

            bound "prev_signal" 0 5

            bound "next_signal" 0 5
            |]
        )

let nothing_ever_happens<'istate when 'istate : comparison> : Automata<'istate,interp> -> SimpleAutomata<int, interp * ('istate option)> =     
    sim 
        (bound "next_path" 1 2 &&& bound "next_signal" 3 4)
        (context.MkAnd 
            [|
            (* Straight from the paper *)
            (* Path rules *)
            (((var "prev_path") ) === (var "next_path"))

            (* Signal rules *)
            ((var "next_signal" ) === var "prev_signal")

            bound "prev_path" 0 5

            bound "next_path" 0 5

            bound "prev_signal" 0 5

            bound "next_signal" 0 5
            |]
        )  
        
        
let simple_automata_B_0<'istate when 'istate : comparison> : Automata<'istate,interp> -> SimpleAutomata<int, interp * ('istate option)> =     
    sim 
        (bound "next_path" 1 2 &&& bound "next_signal" 3 4 &&& bound "next_se" 3 4 &&& bound "next_receptor" 0 1 &&& bound "next_ds1" 0 1 &&& bound "next_ds2" 0 1 &&& bound "next_ds3" 0 1)
        (context.MkAnd 
            [|
            (* Straight from the paper *)
            (* Receptor rules (LET23)*)
            var "prev_input" === int 0 ==> (var "next_receptor" === int 0)
            var "prev_input" === int 1 ==> (var "next_receptor" === int 1)
            (* Downstream effector rules (SEM5, LET60, MAPK) *)
            var "prev_receptor" === int 0 ==> (var "next_ds1" === int 0)
            var "prev_receptor" === int 1 ==> (var "next_ds1" === int 1)
            var "prev_ds1" === int 0 ==> (var "next_ds2" === int 0)
            var "prev_ds1" === int 1 ==> (var "next_ds2" === int 1)
            var "prev_ds2" === int 0 ==> (var "next_ds3" === int 0)
            var "prev_ds2" === int 1 ==> (var "next_ds3" === int 1)
            (* Signal effector rules (LST) *)
            var "prev_signal" === int 0 ==> (var "next_se" === int 0)
            var "prev_signal" === int 1 ==> (var "next_se" === int 1)
            var "prev_signal" === int 2 ==> (var "next_se" === int 2)
            var "prev_signal" === int 3 ==> (var "next_se" === int 3)
            var "prev_signal" === int 4 ==> (var "next_se" === int 4)
            (* Path rules *)
            ((var "prev_path" << int 4) &&& (int 0 << var "prev_path") &&& (var "next_se" << int 4) &&& (var "prev_ds3" === int 0))
                //==> (((var "prev_path") ++ (int 1)) === (var "next_path"))
                ==> (((var "prev_path") ) === (var "next_path"))

            ((var "prev_path" << int 4) &&& (int 0 << var "prev_path") &&& (var "next_se" << int 4) &&& (var "prev_ds3" === int 1))
                ==> (int 4 === (var "next_path"))

            ((var "prev_path" << int 4) &&& (int 0 << var "prev_path") &&& (var "next_se" === int 4))
                ==> (int 0 === (var "next_path"))
            (* Signal rules *)
            (((var "prev_left_path" === int 4) ||| (var "prev_right_path" === int 4)) &&& (int 0 << var "prev_signal"))
                ==> (var "next_signal" === int 4)

            ((var "prev_left_path" << int 4)  &&& (var "prev_right_path" << int 4)  &&& (int 4 === var "prev_path"))
                ==> (var "next_signal" === int 0)

            ((var "prev_left_path" << int 4)  &&& (var "prev_right_path" << int 4)  &&& (var "prev_path" << int 4) &&& (int 0 << var "prev_signal") &&& (var "prev_signal" << int 4))
                //==> ((var "next_signal" ++ int 1) === var "prev_signal")
                ==> ((var "next_signal" ) === var "prev_signal")

            (* Rules for dealing with not performing an update*)
            ((var "prev_left_path" << int 4)  &&& (var "prev_right_path" << int 4)  &&& (var "prev_path" << int 4) &&& (var "prev_signal" === int 4))
                ==> (int 4 === var "prev_signal")

            ((var "prev_signal" === int 0)  ==> (var "next_signal" === int 0))

            ((var "prev_path" === int 0) ||| (var "prev_path" === int 4)) 
                ==> (var "prev_path" === var "next_path")

            ((var "prev_path" << int 5) &&& ((int 0 << var "prev_path") ||| (var "prev_path" === int 0)))

            bound "prev_path" 0 5

            bound "next_path" 0 5

            bound "prev_signal" 0 5

            bound "next_signal" 0 5
            |]
        )  

let simple_automata_B_1<'istate when 'istate : comparison> : Automata<'istate,interp> -> SimpleAutomata<int, interp * ('istate option)> =     
    //Variables are path, input, signal,vul and notch
    sim 
        //Initialise everything as off/0
        ( bound "next_path" 0 1 &&& bound "next_signal" 0 1 &&& bound "next_lst" 0 1 &&& bound "next_let23" 0 1 &&& bound "next_sem5" 0 1 &&& bound "next_sur2" 0 1 &&& bound "next_let60" 0 1 &&& bound "next_mapk" 0 1)
        (context.MkAnd 
            [|
            (* Straight from the paper *)
            (* Receptor rules (LET23)*)
            var "prev_input" === int 0 ==> (var "next_let23" === int 0)
            var "prev_input" === int 1 ==> (var "next_let23" === int 1)
            (* Downstream effector rules (SEM5, LET60, MAPK, SUR2) *)
            var "prev_let23" === int 0 ==> (var "next_sem5" === int 0)
            var "prev_let23" === int 1 ==> (var "next_sem5" === int 1)
            var "prev_sem5" === int 0 ==> (var "next_let60" === int 0)
            var "prev_sem5" === int 1 ==> (var "next_let60" === int 1)
            var "prev_let60" === int 0 ==> (var "next_mapk" === int 0)
            (var "prev_let60" === int 1) &&& (var "prev_lst" === int 0) ==> (var "next_mapk" === int 1)
            (var "prev_let60" === int 1) &&& (var "prev_lst" === int 1) &&& (var "prev_mapk" === int 0) ==> (var "next_mapk" === int 0)
            (var "prev_let60" === int 1) &&& (var "prev_lst" === int 1) &&& (var "prev_mapk" === int 1) ==> (var "next_mapk" === int 1)
            var "prev_mapk" === int 0 ==> (var "next_sur2" === int 0)
            var "prev_mapk" === int 1 ==> (var "next_sur2" === int 1)
            (* Signal effector rules (LST) *)
            var "prev_signal" === int 0 ==> (var "next_lst" === int 0)
            var "prev_signal" === int 1 ==> (var "next_lst" === int 1)
            (* Path (lateral signal) rules *)
            var "prev_mapk" === int 0 ==> (var "next_path" === int 0)
            var "prev_mapk" === int 1 ==> (var "next_path" === int 1)
            (* Signal rules *)
            //var "next_signal" === var "prev_right_path"
            (var "prev_left_path" === int 0) &&& (var "prev_right_path" === int 0) ==> (var "next_signal" === int 0)
            ((var "prev_left_path" === int 1) &&& (var "prev_right_path" === int 0)) &&& (var "prev_sur2" === int 0) ==> (var "next_signal" === int 1)
            ((var "prev_left_path" === int 0) &&& (var "prev_right_path" === int 1)) &&& (var "prev_sur2" === int 0) ==> (var "next_signal" === int 1)
            ((var "prev_left_path" === int 1) &&& (var "prev_right_path" === int 0)) &&& (var "prev_sur2" === int 1) &&& (var "prev_signal" === int 0) ==> (var "next_signal" === int 0)
            ((var "prev_left_path" === int 0) &&& (var "prev_right_path" === int 1)) &&& (var "prev_sur2" === int 1) &&& (var "prev_signal" === int 1) ==> (var "next_signal" === int 1)
            (* Fate  rules *)

            bound "prev_path" 0 2

            bound "next_path" 0 2

            bound "prev_signal" 0 2

            bound "next_signal" 0 2
            |]
        )  


let simple_automata_B_1_forms =     
    //Variables are path, input, signal,vul and notch
        //Initialise everything as off/0
        ( bound "next_path" 0 1 &&& bound "next_signal" 0 1 &&& bound "next_lst" 0 1 &&& bound "next_let23" 0 1 &&& bound "next_sem5" 0 1 &&& bound "next_sur2" 0 1 &&& bound "next_let60" 0 1 &&& bound "next_mapk" 0 1),
        (context.MkAnd 
            [|
            (* Straight from the paper *)
            (* Receptor rules (LET23)*)
            var "prev_input" === int 0 ==> (var "next_let23" === int 0)
            var "prev_input" === int 1 ==> (var "next_let23" === int 1)
            (* Downstream effector rules (SEM5, LET60, MAPK, SUR2) *)
            var "prev_let23" === int 0 ==> (var "next_sem5" === int 0)
            var "prev_let23" === int 1 ==> (var "next_sem5" === int 1)
            var "prev_sem5" === int 0 ==> (var "next_let60" === int 0)
            var "prev_sem5" === int 1 ==> (var "next_let60" === int 1)
            var "prev_let60" === int 0 ==> (var "next_mapk" === int 0)
            (var "prev_let60" === int 1) &&& (var "prev_lst" === int 0) ==> (var "next_mapk" === int 1)
            (var "prev_let60" === int 1) &&& (var "prev_lst" === int 1) &&& (var "prev_mapk" === int 0) ==> (var "next_mapk" === int 0)
            (var "prev_let60" === int 1) &&& (var "prev_lst" === int 1) &&& (var "prev_mapk" === int 1) ==> (var "next_mapk" === int 1)
            var "prev_mapk" === int 0 ==> (var "next_sur2" === int 0)
            var "prev_mapk" === int 1 ==> (var "next_sur2" === int 1)
            (* Signal effector rules (LST) *)
            var "prev_signal" === int 0 ==> (var "next_lst" === int 0)
            var "prev_signal" === int 1 ==> (var "next_lst" === int 1)
            (* Path (lateral signal) rules *)
            var "prev_mapk" === int 0 ==> (var "next_path" === int 0)
            var "prev_mapk" === int 1 ==> (var "next_path" === int 1)
            (* Signal rules *)
            //var "next_signal" === var "prev_right_path"
            (var "prev_left_path" === int 0) &&& (var "prev_right_path" === int 0) ==> (var "next_signal" === int 0)
            ((var "prev_left_path" === int 1) &&& (var "prev_right_path" === int 0)) &&& (var "prev_sur2" === int 0) ==> (var "next_signal" === int 1)
            ((var "prev_left_path" === int 0) &&& (var "prev_right_path" === int 1)) &&& (var "prev_sur2" === int 0) ==> (var "next_signal" === int 1)
            ((var "prev_left_path" === int 1) &&& (var "prev_right_path" === int 0)) &&& (var "prev_sur2" === int 1) &&& (var "prev_signal" === int 0) ==> (var "next_signal" === int 0)
            ((var "prev_left_path" === int 0) &&& (var "prev_right_path" === int 1)) &&& (var "prev_sur2" === int 1) &&& (var "prev_signal" === int 1) ==> (var "next_signal" === int 1)
            (* Fate  rules *)

            bound "prev_path" 0 2

            bound "next_path" 0 2

            bound "prev_signal" 0 2

            bound "next_signal" 0 2
            |]
        )  
