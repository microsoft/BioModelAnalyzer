using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BioCheck.Web.Analysis
{
    public struct CounterExampleTypes
    {
        public const string Bifurcation = "Bifurcation";
        public const string Oscillation = "Oscillation";
    }

    public class BifurcatingVariableOutput
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string CalculatedBound { get; set; }

        public int Fix1 { get; set; }

        public int Fix2 { get; set; }
    }

    public class OscillatingVariableOutput
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string CalculatedBound { get; set; }

        public string Oscillation { get; set; }
    }

    public class CounterExampleOutput
    {
        public string Status { get; set; }

        public string CounterExampleType { get; set; }

        public List<BifurcatingVariableOutput> BifurcatingVariables { get; set; }

        public List<OscillatingVariableOutput> OscillatingVariables { get; set; }

        public string Error { get; set; }

        [DataMember()]
        public byte[] ZippedXml { get; set; }
    }

    public class CounterExampleOutputDTO
    {
        public string Status { get; set; }

        public string CounterExampleType { get; set; }

        public string Error { get; set; }

        [DataMember()]
        public byte[] ZippedXml { get; set; }
    }

    public class FurtherTestingOutputDTO
    {
        public FurtherTestingOutputDTO()
        {
            this.CounterExamples = new List<CounterExampleOutput>();
            this.ErrorMessages = new List<string>();
        }

        public List<CounterExampleOutput> CounterExamples { get; set; }

        public string Error { get; set; }

        public List<string> ErrorMessages { get; set; }

        [DataMember()]
        public byte[] ZippedLog { get; set; }
    }

    public class FurtherTestingOutput
    {
        public FurtherTestingOutput()
        {
            this.CounterExamples = new List<CounterExampleOutput>();
            this.ErrorMessages = new List<string>();
        }

        public List<CounterExampleOutput> CounterExamples { get; set; }

        public string Error { get; set; }

        public List<string> ErrorMessages { get; set; }

        [DataMember()]
        public byte[] ZippedLog { get; set; }
    }
}