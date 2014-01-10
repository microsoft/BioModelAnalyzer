(* Copyright (c) Microsoft Corporation. All rights reserved. *)
module UnitTests

open System.Xml
open System.Xml.Linq

type IA = BioCheckAnalyzerCommon.IAnalyzer2

let register_tests2 (analyzer:UIMain.Analyzer2) =
    
    let load_run_check (fname:string) ocheck_stabilizes ocheck_cex = 
        XDocument.Load(fname)
        |> Marshal.model_of_xml 
        |> Stabilize.stabilization_prover
        |> (fun (sr,cex_o) -> 
                    match (sr,cex_o) with 
                     | (Result.SRStabilizing(_), None) -> 
                            (Option.get ocheck_stabilizes) sr
                     | (Result.SRNotStabilizing(_), Some(cex)) -> 
                            (Option.get ocheck_cex) cex 
                     | _ -> failwith "UnitTests got bad results from checkStabilityOrElseFindCEx")

    Test.register_test true (fun () -> load_run_check "fs_2var_unstable.xml" None (Some Result.bifurcates))
    Test.register_test true (fun () -> load_run_check "fs_BuddingYeast.xml"  (Some Result.stabilizes) None) 
    Test.register_test true (fun () -> load_run_check "fs_BuddingYeastx2.xml" (Some Result.stabilizes) None) 
    Test.register_test true (fun () -> load_run_check "fs_CaitlinToyModel.xml" (Some Result.stabilizes) None) 
 // Drosophila TO.                     load_run_check
 //   Test.register_test true (fun () -load_run_checkun "fs_DrosophilaPairRule.xml" (Some Result.stabilizes) None) 
    Test.register_test true (fun () -> load_run_check "fs_OnlyInhibitionNonDefault.xml" None (Some Result.cycles)) 
    Test.register_test true (fun () -> load_run_check "fs_OnlyInhibitionNonStabilizing.xml" None (Some Result.cycles)) 
    Test.register_test true (fun () -> load_run_check "fs_OnlyInhibitionTest.xml" None (Some Result.cycles))
    //Test.register_test true (fun () -> load_run_check "fs_SkinModel.xml" (Some Result.stabilizes) None) 
    Test.register_test true (fun () -> load_run_check "fs_Skinv6.xml" (Some Result.stabilizes) None) 
    //Test.register_test true (fun () -> load_run_check "fs_ToyModelStable.xml" (Some Result.stabilizes) None) 
    Test.register_test true (fun () -> load_run_check "fs_ToyModelUnstable.xml" None (Some Result.cycles))     
    Test.register_test true (fun () -> load_run_check "fs_VPC_lin15ko.xml" (Some Result.stabilizes) None) 
    Test.register_test true (fun () -> load_run_check "fs_VPCwildtype_v1.xml" (Some Result.stabilizes) None) 
    Test.register_test true (fun () -> load_run_check "fs_biocheck_ex.xml" None (Some Result.cycles)) // SI: changed from fixpoint
    Test.register_test true (fun () -> load_run_check "fs_biocheck_ex1.xml" (Some Result.stabilizes) None)
    Test.register_test true (fun () -> load_run_check "fs_diabetes_new_mod.xml" (Some Result.stabilizes) None) 

