// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
//
//  Module Name:
//
//      test.fs
//
//  Abstract:
//
//      Infrastructure for unit testing
//
//  Contact:
//
//      Byron Cook (bycook)
//
//  Notes:
//
//      * This is a bit of a hack.  We define register_test as an inlined
//        function, then .net tricks to get the file name and line number
//        where the test was registered.  The point of this is that you do not
//        need to name your tests, they're uniquely defined by where they were
//        registered so long as you dont use register_test within a function
//        that is re-used, or a loop.
//

(*
SI: further work:
    - add capability to TimeOut
    - Use Async to run tests? http://msdn.microsoft.com/en-us/library/dd233250.aspx
      Z3 doesn't work with Async? Or, have to rewrite to launch separate exe for each test. 

*)

module Test



// Set of all unit tests across BioCheck
//                   result,test,            (file,    method,  line)
let tests = ref ([]:(bool * (unit -> bool) * (string * string * int)) list)


// if "register_testd" gets called we'll shut off normal testing and just run
// the one case we're debugging ..........this allows you to easily debug a
// broken regression test by swapping a call to "register_test" with
// "register_testd"
let found_debug = ref false
let register_testd s f =
    Printf.printf "Debugging.............\n"
    Log.print_log := true
    found_debug := true
    let r = f()
    Printf.printf "Expected: %b, Found: %b\n" s r


// run_tests () should be called once register_test has been called on all
// of the desired unit tests
let run_tests () =
    Printf.printf "Running tests--------------------------------------------\n";
    let num = List.length !tests
    let failed = ref []
    // SI: old function, no longer used. 
    let run_test (expected_result,test,info) =
        // This is probably not the right thing to do............
        let (file,name,line) = info
        Printf.printf "Running test %s,%d," file line
        let start_time = System.DateTime.Now

        let b = begin
                try let b = test()
                    if b<>expected_result then
                          Printf.printf "Regression: %s,%d\n" file line
                          Printf.printf "expected_result = %b, Current = %b\n" expected_result b
                          failed := (info,b,expected_result) :: !failed;
                    b
                with e ->
                    Printf.printf "Regression: %s,%d\n" file line
                    Printf.printf "RAISED EXCEPTION!!!!!\n"
                    Printf.printf "expected_result = %b, Current = %s\n" expected_result
                                   "exception"
                    Printf.printf "Exception: %s\n" (e.ToString())
                    failed := (info,false,expected_result) :: !failed;

                    false
        end
        Printf.printf "%A\n" (System.DateTime.Now.Subtract start_time)

    // SI: async version of run_test. Allows timeout. (Should pull timeout out to be a cl flag.) 
    let run_test_async (expected_result,test,info) =
        async {
            let (file,name,line) = info
            Printf.printf "Running test %s,%d," file line
            let start_time = System.DateTime.Now
            let b = begin
                    try let b = test()
                        if b<>expected_result then
                              Printf.printf "Regression: %s,%d\n" file line
                              Printf.printf "expected_result = %b, Current = %b\n" expected_result b
                              failed := (info,b,expected_result) :: !failed;
                        b
                    with e ->
                        Printf.printf "Regression: %s,%d\n" file line
                        Printf.printf "RAISED EXCEPTION!!!!!\n"
                        Printf.printf "expected_result = %b, Current = %s\n" expected_result
                                       "exception"
                        Printf.printf "Exception: %s\n" (e.ToString())
                        failed := (info,false,expected_result) :: !failed;

                        false
            end
            Printf.printf "%A\n" (System.DateTime.Now.Subtract start_time)
        }

    if not !found_debug then
        List.iter 
            (fun t -> 
                // run_test t
                try Async.RunSynchronously(run_test_async t, 10000)
                with exc -> printfn "%s" exc.Message
            )
            !tests
    let failed_len = List.length !failed
    Printf.printf "\n\n----------------------------------------\n\n"
    Printf.printf "TEST RESULTS: %d regressions on %d tests\n" failed_len num
    if failed_len>0 then
        Printf.printf "\n----------------------------------------\n";
        Printf.printf "Failures:\n\n";
        let f ((file,name,line),b,expected_result) =
            Printf.printf "Regression: %s,%d\n" file line
            Printf.printf "expected_result = %b, Current = %b\n\n" expected_result b
        List.iter f !failed



let inline register_test expected_result test =
    let sf = new System.Diagnostics.StackFrame(true)
    let st = new System.Diagnostics.StackTrace(sf)
    let cf = st.GetFrame(0)
    let m = cf.GetMethod()
    let n = m.Name
    let k = cf.GetFileLineNumber()
    let fn = cf.GetFileName()
    tests := !tests @ [(expected_result,test,(fn,n,k))]






