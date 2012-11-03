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
    public interface IAnalyzer
    {
        /// <summary>
        /// Run the proof as a One Shot run, and return the results as an XDocument
        /// </summary>
        /// <param name="input">The AnalysisInput XML</param>
        /// <returns>The AnalysisOutput XML</returns>
        XDocument OneShot(XDocument input);

        /// <summary>
        /// Run the proof as a One Shot run, and return the results as an XDocument
        /// </summary>
        /// <param name="input">The AnalysisInput XML</param>
        /// <param name="logService">The log service.</param>
        /// <returns>The AnalysisOutput XML</returns>
        XDocument OneShot(XDocument input, ILogService logService);
    }
}
