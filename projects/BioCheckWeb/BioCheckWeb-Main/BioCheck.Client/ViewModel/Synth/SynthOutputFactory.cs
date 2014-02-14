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
    public static class SynthOutputFactory
    {
        /// <summary>
        /// Creates the AnalysisInput from the specified model VM.
        /// </summary>
        /// <param name="modelVM">The model VM.</param>
        /// <returns></returns>
        public static SynthOutput Create(AnalysisOutputDTO analysisOutputDto)
        {
            if (analysisOutputDto.ZippedXml == null)
                throw new Exception("No analysis output was returned from the Analyser.");

            var xml = ZipHelper.Unzip(analysisOutputDto.ZippedXml);
            var xdoc = XDocument.Parse(xml);                        // Nothing's done with this!

            var output = new SynthOutput(analysisOutputDto);         // Data used to populate the output class
            output.Output = xdoc.ToString();

            // LTL-specific content. Seek content once SYN actually runs at the back-end.
            output.Status = xdoc.Descendants("AnalysisOutput").Elements("Result").FirstOrDefault().Value;
            //if (output.Status == StatusTypes.True || output.Status == StatusTypes.False)
            //{
            //    // Not populated if Error.
            //    output.Model = xdoc.Descendants("Model").Attributes().ElementAt(0).Value;          
            //}
            //else
            //{
            //    var error = xdoc.Descendants("Error").FirstOrDefault();
            //    output.Error = error != null ? error.AttributeString("Msg") : "There was an error in the synthesizer; the input model could not be stabilized.";
            //}

            return output;
        }

        //private static int ParseId(string value)
        //{
        //    int synthIndex = value.IndexOf('^');
        //    if (synthIndex > -1)
        //    {
        //        // The output is from Z3, which is in “variable-id ^ synth" format. You can ignore the synth after the “^". 
        //        string substring = value.Substring(0, synthIndex);
        //        return Int32.Parse(substring);
        //    }

        //    return Int32.Parse(value);
        //}
    }
}