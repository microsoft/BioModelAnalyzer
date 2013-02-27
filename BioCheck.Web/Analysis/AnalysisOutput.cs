using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Linq;

namespace BioCheck.Web.Analysis
{
    public class AnalysisOutputDTO
    {
        public AnalysisOutputDTO()
        {
            this.ErrorMessages = new List<string>();
        }

        public string Status { get; set; }

        public string Error { get; set; }

        public List<string> ErrorMessages { get; set; }

        public double Time { get; set; }

        [DataMember()]
        public byte[] ZippedXml { get; set; }

        [DataMember()]
        public byte[] ZippedLog { get; set; }
    }

    public class AnalysisOutput
    {
        public AnalysisOutput()
        {
            this.ErrorMessages = new List<string>();
        }

        public string Status { get; set; }

        public string Error { get; set; }

        public List<string> ErrorMessages { get; set; }

        public double Time { get; set; }

        [DataMember()]
        public byte[] ZippedXml { get; set; }

        [DataMember()]
        public byte[] ZippedLog { get; set; }
    }
}