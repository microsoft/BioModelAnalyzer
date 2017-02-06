// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
module UnitTests

open System.Xml
open System.Xml.Linq

(*
SI: further work:
    - Extend to CAV/LTL and Garvit engines.
    - Extend to simulation. 


*)

let register_tests () =
    
    let load_run_check (fname:string) ocheck_stabilizes ocheck_cex = 
        failwith "Need to replace xml"
//
//        XDocument.Load(fname)
//        |> Marshal.model_of_xml 
//        |> Stabilize.stabilization_prover
//        |> (fun (sr,cex_o) -> 
//                    match (sr,cex_o) with 
//                     | (Result.SRStabilizing(_), None) -> 
//                            (Option.get ocheck_stabilizes) sr
//                     | (Result.SRNotStabilizing(_), Some(cex)) -> 
//                            (Option.get ocheck_cex) cex 
//                     | _ -> failwith "UnitTests got bad results from checkStabilityOrElseFindCEx")

    Test.register_test true (fun () -> load_run_check "fs_2var_unstable.xml" None (Some Result.bifurcates))
    Test.register_test true (fun () -> load_run_check "fs_BuddingYeast.xml"  (Some Result.stabilizes) None) 
    Test.register_test true (fun () -> load_run_check "fs_BuddingYeastx2.xml" (Some Result.stabilizes) None) 
    Test.register_test true (fun () -> load_run_check "fs_CaitlinToyModel.xml" (Some Result.stabilizes) None) 
 // Drosophila TO.                     load_run_check
 //   Test.register_test true (fun () -load_run_checkun "fs_DrosophilaPairRule.xml" (Some Result.stabilizes) None) 
    Test.register_test true (fun () -> load_run_check "fs_OnlyInhibitionNonDefault.xml" None (Some Result.cycles)) 
    Test.register_test true (fun () -> load_run_check "fs_OnlyInhibitionNonStabilizing.xml" None (Some Result.cycles)) 
//    Test.register_test true (fun () -> load_run_check "fs_OnlyInhibitionTest.xml" None (Some Result.cycles))
    //Test.register_test true (fun () -> load_run_check "fs_SkinModel.xml" (Some Result.stabilizes) None) 
    Test.register_test true (fun () -> load_run_check "fs_Skinv6.xml" (Some Result.stabilizes) None) 
    //Test.register_test true (fun () -> load_run_check "fs_ToyModelStable.xml" (Some Result.stabilizes) None) 
    Test.register_test true (fun () -> load_run_check "fs_ToyModelUnstable.xml" None (Some Result.cycles))     
    Test.register_test true (fun () -> load_run_check "fs_VPC_lin15ko.xml" (Some Result.stabilizes) None) 
    Test.register_test true (fun () -> load_run_check "fs_VPCwildtype_v1.xml" (Some Result.stabilizes) None) 
    Test.register_test true (fun () -> load_run_check "fs_biocheck_ex.xml" None (Some Result.cycles)) // SI: changed from fixpoint
    Test.register_test true (fun () -> load_run_check "fs_biocheck_ex1.xml" (Some Result.stabilizes) None)
    //Test.register_test true (fun () -> load_run_check "fs_diabetes_new_mod.xml" (Some Result.stabilizes) None) 
    //Demo tests- these are commented out because they are graphical models rather than analysis inputs and BMA can't handle that yet
    (*
    Test.register_test true (fun () -> load_run_check "Demo/Skin1D.xml" (Some Result.stabilizes) None)
    Test.register_test true (fun () -> load_run_check "Demo/Skin1D_unstable.xml" (Some Result.cycles) None)
    Test.register_test true (fun () -> load_run_check "Demo/Bcr-Abl No Feedbacks.xml" (Some Result.cycles) None)
    *)
    //VMCAI paper models. These have been converted from the SMV (included in the folder).
    //Some didn't convert easily so haven't been converted (and are absent), and some are v. slow to analyse (and have been commented out)
    Test.register_test true (fun () -> load_run_check "VMCAI11/ESkin6Fxd.xml" (Some Result.stabilizes) None)
    Test.register_test true (fun () -> load_run_check "VMCAI11/ESkin7Fxd.xml" (Some Result.stabilizes) None)
    Test.register_test true (fun () -> load_run_check "VMCAI11/ESkin8Fxd.xml" (Some Result.stabilizes) None)
    Test.register_test true (fun () -> load_run_check "VMCAI11/Skin2D.xml" (Some Result.stabilizes) None)
    Test.register_test true (fun () -> load_run_check "VMCAI11/Skin2DFxd.xml" (Some Result.stabilizes) None)
    Test.register_test true (fun () -> load_run_check "VMCAI11/Skin3D.xml" (Some Result.stabilizes) None)
