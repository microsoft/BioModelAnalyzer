Status of models (29/11/2011):
------------------------------
2var_unstable  --- Goes unstable as expected.
diabetes_newl   --- Analysis input is badly formed: 
                    var names not replaced by ids.
                    This is due to connectivity problems in the model
                    (fixed below).
diabetes_new_mod --- Fixed version of diabetes_new. 
                     No errors on reading.
                     Stabilizes (seems very prone to stabilization).
OnlyInhibitionNonDefault --- Model should not stabilize!
			     Model indeed does not stabilize.
OnlyInhibitionNonStabilizing --- Model not stabilizing as expected.
OnlyInhibitionTest --- Model shows why the default target function for 
                       variables with only inhibition does not make sense.
                       Stabilizes (as should).
Skin_v6 --- Reads fine.
            Stabilizes. SHOULD BE CHECKED THOROUGHLY (BUT PROBABLY DOES).

              *****************************************
VPCcausingbug ---
                  Reads and displays fine.
                  Errors in transfer to analysis.
                  Final state.

VPCwildtype_v1 ---
                   Reads and displays fine.
                   Analysis returns stabilization.
                   SHOULD BE CHECKED THOROUGHLY (BUT PROBABLY DOES).
 
                   *******************************************

Invalid models
==============
budding yeast.xml  --- Error on loading: "Object reference not set to an instance of an object"
diabetes8d_v1.xml  --- Error on loading: "Object reference not set to an instance of an object"
skin_modified.xml  --- Error on loading: "Missing XML element: Name"

QinsiSkinModels
===============
Skin1D ---
           Stabilizes. No errors.
                   SHOULD BE CHECKED THOROUGHLY (BUT PROBABLY DOES).
 
                   *******************************************
Skin2D_3Cells_2layers ---
           Stabilizes. No errors.
                   SHOULD BE CHECKED THOROUGHLY (BUT PROBABLY DOES).
 
                   *******************************************
Skin2D_5X2 ---
               Problem with definition of target functions for Notch-IC.
               There is no relationship with ligand-in
               There are too many ligand-in and I don't know how to connect them.
SSkin1D ---
           Stabilizes. No errors.
                   SHOULD BE CHECKED THOROUGHLY (BUT PROBABLY DOES).
 
                   *******************************************
SSkin2D_3cells_2layers ---
               Problem with definition of target functions for Notch-IC.
               There is no relationship with ligand-in
               There are too many ligand-in and I don't know how to connect them.
