using BioModelAnalyzer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace bma.LTLPolarity
{
    public enum LTLStatus
    {
        False,
        True,
        Unknown
    }

    public class LTLAnalysisResult
    {
        public LTLStatus Status { get; set; }

        /// <summary>Additional error information if status is nor Stabilizing neither NonStabilizing</summary>
        [XmlIgnore]
        public string Error { get; set; }

        [XmlElement("Tick", Type = typeof(Tick))]
        public Tick[] Ticks { get; set; }

        public string[] ErrorMessages { get; set; }

        public string[] DebugMessages { get; set; }

        [XmlElement("Loop", Type = typeof(int))]
        public int Loop { get; set; }
    }

    public class LTLPolarityAnalysisInputDTO : Model
    {
        [XmlIgnore]
        public bool EnableLogging { get; set; }
        public string Formula { get; set; }

        public string Number_of_steps { get; set; }

        public LTLStatus Polarity { get; set; }
    }
}
