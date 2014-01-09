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
    public static class TimeOutputFactory
    {
        /// <summary>
        /// Creates the AnalysisInput from the specified model VM.
        /// </summary>
        /// <param name="modelVM">The model VM.</param>
        /// <returns></returns>
        public static TimeOutput Create(AnalysisOutputDTO analysisOutputDto)
        {
            if (analysisOutputDto.ZippedXml == null)
                throw new Exception("No analysis output was returned from the Analyser.");

            var xml = ZipHelper.Unzip(analysisOutputDto.ZippedXml);
            var xdoc = XDocument.Parse(xml);

            var output = new TimeOutput(analysisOutputDto);         // Data used to populate the output class

            output.Status = xdoc.Descendants("Status").FirstOrDefault().Value;

            if (output.Status == StatusTypes.True || output.Status == StatusTypes.False)
            {
                // Not populated if Error.
                output.Model = xdoc.Descendants("Model").Attributes().ElementAt(0).Value;          
            }
            else
            {
                var error = xdoc.Descendants("Error").FirstOrDefault();
                output.Error = error != null ? error.AttributeString("Msg") : "There was an error in the ltl analyzer; the input formula was neither proven to be True or False.";
            }

            return output;
        }

        //private static int ParseId(string value)
        //{
        //    int timeIndex = value.IndexOf('^');
        //    if (timeIndex > -1)
        //    {
        //        // The output is from Z3, which is in “variable-id ^ time" format. You can ignore the time after the “^". 
        //        string substring = value.Substring(0, timeIndex);
        //        return Int32.Parse(substring);
        //    }

        //    return Int32.Parse(value);
        //}
    }
}