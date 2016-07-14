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
    public class AnalysisOutput : AnalysisResult
    {
        public int Time { get; set; }

        public string[] ErrorMessages { get; set; }

        public string[] DebugMessages { get; set; }
    }

    public class AnalysisInput : Model
    {
        public bool EnableLogging { get; set; }
    }

    public static class Analysis
    {
        public static AnalysisOutput Analyze(AnalysisInput input)
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
                var result = analyzer.checkStability(input);
                sw.Stop();

                log.LogDebug(string.Format("The analysis took {0}", sw.Elapsed));

                if (result.Status != StatusType.Stabilizing && result.Status != StatusType.NotStabilizing)
                    throw new Exception("The stability status is neither 'Stabilizing' nor 'NotStabilizing'; result error: " + (result.Error == null ? "<null>" : result.Error));

                return new AnalysisOutput
                {
                    Error = result.Error,
                    Ticks = result.Ticks,
                    Status = result.Status,
                    Time = (int)Math.Round(sw.Elapsed.TotalSeconds, 1),
                    ErrorMessages = log.ErrorMessages.Length > 0 ? log.ErrorMessages.ToArray() : null,
                    DebugMessages = log.DebugMessages.Length > 0 ? log.DebugMessages.ToArray() : null
                };
            }
            catch (Exception ex)
            {
                var version = typeof(Analysis).Assembly.GetName().Version;
                log.LogError(String.Format("Analysis failed. Assembly version: {0}. Exception: {1}", version, ex));
                // Return an Unknown if fails
                return new AnalysisOutput
                {
                    Status = StatusType.Error,
                    Error = ex.Message,
                    ErrorMessages = log.ErrorMessages.Length > 0 ? log.ErrorMessages.ToArray() : null,
                    DebugMessages = log.DebugMessages.Length > 0 ? log.DebugMessages.ToArray() : null
                };
            }
        }
    }
}
