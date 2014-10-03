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

        // engine interfaces: 

        // 1. VMCAI interface
        // Max time (in O(n), not necessarily seconds) to check stability.
        int complexity(XDocument input_model);

        // The VMCAI engine is broken up into the checkStability part and the subsequent (if needed) 
        // findCEx parts. This decoupling allows the UI to run just the first (always pretty fast), 
        // giving the user the option of not needing to run the second (mostly timeout). 

        // checkStability takes a analyzer input model, and returns whether the model stabilizes or not.
        XDocument checkStability(XDocument input_model);
        // In case the model doesn't stabilize, then find a counter-example. 
        XDocument findCExBifurcates(XDocument input_model, XDocument notstabilizing_result);
        XDocument findCExCycles(XDocument input_model, XDocument notstabilizing_result);
        XDocument findCExFixpoint(XDocument input_model, XDocument notstabilizing_result);

        // 2. CAV interface
        XDocument checkLTL(XDocument input_model, string formula, string num_of_steps);

        // 3. SYN interface
        XDocument checkSynth(XDocument input_model);

        // 4. SCM interface 
        XDocument checkSCM(XDocument input_model);

        // 5. Simulation Interface.
        // Given initial_env (bindings of variable to a value), return the env at the next tick. 
        System.Collections.Generic.Dictionary<int, int> simulate_tick(XDocument input_model, System.Collections.Generic.Dictionary<int, int> initial_env);
    }

}




