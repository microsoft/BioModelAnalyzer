using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

using Microsoft.FSharp.Core;

using BioModelAnalyzer;

namespace BioCheckAnalyzerCommon
{

    /// <summary>
    /// Interface for the Analyzer class, the entry point into the BioCheckAnalyzer project
    /// </summary>

    public interface IAnalyzer
    {
        // Logging interface. 
        void LoggingOn(ILogService logger);
        void LoggingOff();

        // engine interfaces: 

        // 1. VMCAI interface
        // Max time (in O(n), not necessarily seconds) to check stability.
        int complexity(Model input_model);

        // The VMCAI engine is broken up into the checkStability part and the subsequent (if needed) 
        // findCEx parts. This decoupling allows the UI to run just the first (always pretty fast), 
        // giving the user the option of not needing to run the second (mostly timeout). 

        // checkStability takes a analyzer input model, and returns whether the model stabilizes or not.
        AnalysisResult checkStability(Model input_model);
        // In case the model doesn't stabilize, then find a counter-example. 
        FSharpOption<BifurcationCounterExample> findCExBifurcates(Model input_model, AnalysisResult notstabilizing_result);
        FSharpOption<CycleCounterExample> findCExCycles(Model input_model, AnalysisResult notstabilizing_result);
        FSharpOption<FixPointCounterExample> findCExFixpoint(Model input_model, AnalysisResult notstabilizing_result);

        // 2. CAV interface
        XDocument checkLTL(Model input_model, string formula, string num_of_steps);

        // 3. SYN interface
        XDocument checkSynth(Model input_model);

        // 4. SCM interface 
        XDocument checkSCM(Model input_model);

        // 5. Simulation Interface.
        // Given initial_env (bindings of variable to a value), return the env at the next tick. 
        SimulationVariable[] simulate_tick(Model input_model, SimulationVariable[] initial_env);
    }
}




