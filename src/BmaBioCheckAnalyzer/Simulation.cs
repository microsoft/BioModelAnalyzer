// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
using BioCheckAnalyzerCommon;
using BioModelAnalyzer;
using bma.Diagnostics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bma.BioCheck
{
    public class SimulationInput
    {
        public Model Model { get; set; }

        public SimulationVariable[] Variables { get; set; }

        public bool EnableLogging { get; set; }
    }

    public class SimulationOutput
    {
        public SimulationVariable[] Variables { get; set; }

        public string[] ErrorMessages { get; set; }

        public string[] DebugMessages { get; set; }
    }

    public static class Simulation
    {
        public static SimulationOutput Simulate(SimulationInput input)
        {
            var log = new LogService();

            try
            {
                IAnalyzer analyzer = new UIMain.Analyzer();
                if (input.EnableLogging)
                {
                    analyzer.LoggingOn(log);
                }
                else
                {
                    analyzer.LoggingOff();
                    log.LogDebug("Logging is disabled.");
                }

                Stopwatch sw = new Stopwatch();
                sw.Start();
                var output = analyzer.simulate_tick(input.Model, input.Variables);
                sw.Stop();

                log.LogDebug(string.Format("The simulation took {0}", sw.Elapsed));

                return new SimulationOutput
                {
                    Variables = output,
                    ErrorMessages = log.ErrorMessages.Length > 0 ? log.ErrorMessages.ToArray() : null,
                    DebugMessages = log.DebugMessages.Length > 0 ? log.DebugMessages.ToArray() : null,
                };
            }
            catch (Exception ex)
            {
                var version = typeof(Simulation).Assembly.GetName().Version;
                log.LogError(String.Format("Simulation failed. Assembly version: {0}. Exception: {1}", version, ex));
                return new SimulationOutput
                {
                    ErrorMessages = log.ErrorMessages.Length > 0 ? log.ErrorMessages.ToArray() : null,
                    DebugMessages = log.DebugMessages.Length > 0 ? log.DebugMessages.ToArray() : null
                };
            }
        }
    }
}
