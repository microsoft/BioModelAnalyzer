using System;
using System.Linq;
using System.Xml.Linq;
using BioCheck.AnalysisService;
using BioCheck.Helpers;
using MvvmFx.Common.Helpers;

namespace BioCheck.ViewModel.Proof
{
    /// <summary>
    /// Static Factory class for converting a ModelViewModel to AnalysisInput
    /// </summary>
    public static class AnalysisOutputFactory
    {
        /// <summary>
        /// Creates the AnalysisInput from the specified model VM.
        /// </summary>
        /// <param name="modelVM">The model VM.</param>
        /// <returns></returns>
        public static AnalysisOutput Create(AnalysisOutputDTO analysisOutputDto)
        {
            if(analysisOutputDto.ZippedXml == null)
                throw new Exception("No analysis output was returned from the Analyser.");

            var xml = ZipHelper.Unzip(analysisOutputDto.ZippedXml);
            var xdoc = XDocument.Parse(xml);

            var output = new AnalysisOutput(analysisOutputDto);
            output.Time = analysisOutputDto.Time;

            output.Status = xdoc.Descendants("Status").FirstOrDefault().Value;

            if (output.Status == StatusTypes.Stabilizing)
            {
                // Stabilizing
                // All the variables are stable
                // The value returned with each variable is the Value and should be displayed

                output.Ticks = (from t in xdoc.Descendants("Tick")
                                select new AnalysisTick
                                {
                                    Time = t.ElementInt("Time"),
                                    Variables = (from v in t.Descendants("Variable")
                                                 let lo = v.AttributeInt("Lo")
                                                 let hi = v.AttributeInt("Hi")
                                                 select new VariableOutput
                                                            {
                                                                Id = ParseId(v.AttributeString("Id")),
                                                                Low = lo,
                                                                High = hi,
                                                                IsStable = lo == hi
                                                            }).ToList()
                                })
                                .OrderBy(t => t.Time).ToList();
            }
            else if (output.Status == StatusTypes.NotStabilizing)
            {
                output.Ticks = (from t in xdoc.Descendants("Tick")
                                select new AnalysisTick
                                           {
                                               Time = t.ElementInt("Time"),
                                               Variables = (from v in t.Descendants("Variable")
                                                            let lo = v.AttributeInt("Lo")
                                                            let hi = v.AttributeInt("Hi")
                                                            select new VariableOutput
                                                                       {
                                                                           Id = ParseId(v.AttributeString("Id")),
                                                                           Low = lo,
                                                                           High = hi,
                                                                           IsStable = (lo == hi)
                                                                       }).ToList()
                                           })
                    .OrderBy(t => t.Time).ToList();
            }
            else
            //if (output.Status == StatusTypes.Error ||
            // output.Status == StatusTypes.Default ||
            // output.Status == StatusTypes.TryingStabilizing ||
            // output.Status == StatusTypes.Unknown)
            {
                // SCM edit: no error, just alternate state
                if (output.Status == "SingleStablePoint" || output.Status == "MultiStablePoints" || output.Status == "Cycle" )
                {
                    // Add on the zipped Details
                    output.Status += " ";
                    output.Status += xdoc.Descendants("Details").FirstOrDefault().Value;
                }
                else 
                {
                    var error = xdoc.Descendants("Error").FirstOrDefault();
                    output.Error = error != null ? error.AttributeString("Msg") : "There was an error in the analyzer";
                }
                
            }

            return output;
        }

        private static int ParseId(string value)
        {
            int timeIndex = value.IndexOf('^');
            if (timeIndex > -1)
            {
                // The output is from Z3, which is in “variable-id ^ time" format. You can ignore the time after the “^". 
                string substring = value.Substring(0, timeIndex);
                return Int32.Parse(substring);
            }

            return Int32.Parse(value);
        }
    }
}