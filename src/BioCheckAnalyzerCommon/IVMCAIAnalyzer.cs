using BioCheckAnalyzerCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioModelAnalyzer
{
    public interface IVMCAIAnalyzer
    {
        AnalysisResult CheckStability(Model model, ILogService logger);

        CycleCounterExample[] FindCycleCex(Model model, AnalysisResult result, ILogService logger);

        BifurcationCounterExample[] FindBifurcationCex(Model model, AnalysisResult result, ILogService logger);

        FixPointCounterExample[] FindFixPointCex(Model model, AnalysisResult result, ILogService logger);

        SimulationVariable[] Simulate(Model model, SimulationVariable[] variables, ILogService logger);
    }
}
