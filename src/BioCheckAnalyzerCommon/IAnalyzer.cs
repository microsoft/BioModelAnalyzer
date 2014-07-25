using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace BioCheckAnalyzerCommon
{

    /// <summary>
    /// Interface for the Analyzer class, the entry point into the BioCheckAnalyzer project
    /// </summary>

    // Sample VMCAI interaction from UI:
    //   // Given an xml model, check stability. 
    //   var xml_model  = ...;
    //   var xml_result = IA.checkStability(xml_model);
    //   // Later, if checkStability said that the model was unstable, we find 
    //   // a counter-example. 
    //   var xml_cex_bifurcates = IA.findCExBifurcates(xml_model,xml_result);
    //                                                           ^ note: we pass the not_stable result back in.
    //   var xml_cex_cycles = IA.findCExCycles(xml_model,xml_result);
    
    //
    // All functions that take xml input might raise exceptions if the input is badly formed. 
    public interface IAnalyzer2
    {
        // Logging interface. 
        void LoggingOn(ILogService logger);
        void LoggingOff();
        // // VMCAI Interface.
        // Max time (in O(n), not necessarily seconds) to check stability.
        int complexity(XDocument input_model);
        // checkStability takes a analyzer input model, and returns whether the model stabilizes or not.
        XDocument checkStability(XDocument input_model);
        // In case the model doesn't stabilize, then find a counter-example. 
        XDocument findCExBifurcates(XDocument input_model, XDocument notstabilizing_result);
        XDocument findCExCycles(XDocument input_model, XDocument notstabilizing_result);
        XDocument findCExFixpoint(XDocument input_model, XDocument notstabilizing_result);
        // // CAV (LTL) interface
        XDocument checkLTL(XDocument input_model, string formula, string num_of_steps);
        
        // SYN interface test / dahl
        XDocument checkSynth(XDocument input_model);

        // // Simulation Interface.
        // Given initial_env (bindings of variable to a value), return the env at the next tick. 
        System.Collections.Generic.Dictionary<int, int> simulate_tick(XDocument input_model, System.Collections.Generic.Dictionary<int, int> initial_env);
    }
    
}




