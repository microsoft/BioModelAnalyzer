module Simulator
open Automata
open Microsoft.Z3
open System.Collections.Generic

type formula = Microsoft.Z3.BoolExpr
type interp = Map<string, int>

let context = new Microsoft.Z3.Context()
let (==>) x y = context.MkImplies (x,y)
let (&&&) x y = context.MkAnd(x,y)
let (|||) x y = context.MkOr(x,y)
let (===) x y = context.MkEq(x,y)
let (<<)  x y = context.MkLt(x,y)
let (++)  x y = context.MkAdd(x,y)

let not x = context.MkNot(x)
let int (i : int) = context.MkInt(i)
let var (s : string) = context.MkIntConst(s)
    
let next_pre = "next_"
let prev_pre = "prev_"

///Returns the interpretation of the new state in this model
let get_interp (m : Model) : interp = 
    let mutable interp = Map.empty
    for fd in m.ConstDecls do
        let constant = fd.Name.ToString()
        if constant.StartsWith(next_pre) then
             match m.ConstInterp fd with
             | :? IntNum as i  -> 
                let constant = constant.Substring(next_pre.Length)
                interp <- Map.add constant i.Int interp
             | _ -> ()
    interp

///Creates the formula representing this variable assignment
///  interp_form i  s
/// i is the variable to value map
/// s is a string to prepend onto each variable
let interp_form (i : interp) s : formula = 
    let assigns = 
        [| 
            for j,k in Map.toSeq i do
                yield var (s + j) === int k
        |]
    context.MkAnd(assigns)

let cache (f : 'a -> 'b) =
    let cache = new System.Collections.Generic.Dictionary<'a, 'b>()
    fun x -> 
        match cache.TryGetValue(x) with
        | true, interps -> interps 
        | false, _ ->
            let v = f x 
            cache.Add(x,v)
            v

let normalize_gen () =
    let table = new System.Collections.Generic.Dictionary<_,_>()
    let index = ref 0
    fun x -> 
        match table.TryGetValue x with
        | true, y -> y
        | false, _ -> 
            let r = !index
            table.Add(x,r)
            index := r + 1
            r

///Stateful function to provide an all_smt loop given an interpretation of some of the variables
///Recommended use
///    let foo = all_smt foo_form
///    let r1  = foo i1
///    let r2  = foo i2
///    ...
let all_smt (f : formula) : interp list -> ICollection<interp> =
    //Construct Z3 context with the formula
    let solver = context.MkSimpleSolver()
    solver.Assert(f)
    cache (
      fun base_interps ->
        //Push interpretation of some variables to formula
        solver.Push()
        for i in base_interps do
            solver.Assert(interp_form i prev_pre)
        //Loop finding new interpretations of remaining variables 
        let mutable new_interps = System.Collections.Generic.HashSet<_>() 
        while solver.Check([||]) = Microsoft.Z3.Status.SATISFIABLE do
            //Evaluate the interpretation
            let next_interp = get_interp solver.Model
            if new_interps.Add next_interp then
                ()
            else 
                printfn "Under constained input to Z3 formula"
                assert false
            //Assert negation of interpretation
            solver.Assert(context.MkNot (interp_form next_interp next_pre))

        //Clear the interpretation
        solver.Pop()
        //Return all the interpretations
        new_interps :> ICollection<interp>
    )

///Stateful function to simulate stuff.  The first two arguments establish the Z3 context, so recommended use is
///  let sim1 = sim initform stepformula
///  let t1 = sim1 R0
///  let t2 = sim1 R1
///  ...
///  let t3 = sim1 Rn 
let sim initform stepformula =
    //Prebuild set of initial states from formula
    let init_interps = all_smt initform []

    //Build a context for getting the steps, but delay the interpretation allows reuse
    let steps = all_smt stepformula

    fun (rely : Automata<'istate,interp>) ->
        ///Stateful function to normalize the states to integers
        let normalize = normalize_gen ()
        //The mutable automata we will return for this execution
        let result = new SimpleAutomata<int, (interp * 'istate)>()
        
        //Add the initial states
        for interp in init_interps do
            for index in rely.initialstates do
                result.addInitialState (normalize (interp,index))
                result.addState((normalize (interp,index)), (interp,index))
    
        //Set up work list.
        let work_set = System.Collections.Concurrent.ConcurrentBag()
        for v in result.initialstates do 
            work_set.Add (result.value v)
    
        //Mutable variables for working with the work_set.
        //The ref param in C# makes this harder to work with
        let mutable more, interp_index = work_set.TryTake()
        while more do
            let index = snd interp_index
            let interp = fst interp_index
            //Find next states in the rely
            let new_indexs = rely.next(index)
            //For all interpretations of the new variables
            for new_interp in steps [interp ; rely.value index] do
                //For all next indexs of the rely
                for new_index in new_indexs do
                    //Add an edge for the reduction
                    result.addEdge ((normalize (interp,index)), (normalize (new_interp,new_index)))
                    //Check if the target is new
                    if Set.contains (normalize (new_interp,new_index)) result.states then
                        ()
                    else
                        //If it is new, add the state, and add to work set.
                        result.addState ((normalize (new_interp,new_index)), (new_interp,new_index))
                        work_set.Add( (new_interp,new_index) )
            more <- work_set.TryTake(&interp_index)

        result


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
let test_automata3<'istate when 'istate : comparison> : Automata<'istate,interp> -> SimpleAutomata<int, interp * 'istate> =     

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
let test_automata4<'istate when 'istate : comparison> : Automata<'istate,interp> -> SimpleAutomata<int, interp * 'istate> =     

    let bound f l h =  ((var f << int h) &&& ((int l << var f) ||| (var f === int l)))
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
let test_automata5<'istate when 'istate : comparison> : Automata<'istate,interp> -> SimpleAutomata<int, interp * 'istate> =     

    let bound f l h =  ((var f << int h) &&& ((int l << var f) ||| (var f === int l)))
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
        

