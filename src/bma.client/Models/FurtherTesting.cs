using bma.client;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Xml.Serialization;
namespace bmaclient
{
    public class FurtherTestingInput 
    {
        public AnalysisInput Model { get; set; }

        public AnalysisOutput Analysis { get; set; }

        public bool EnableLogging { get; set; }
    }

    public enum CounterExampleType
    {
        Bifurcation, Cycle, Fixpoint
    }
/*
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
*/


    public class CounterExampleOutput    
    {
        public class CounterExampleVariables
        {
            [XmlElement("Variable", Type = typeof(CounterExampleVariable))]
            public CounterExampleVariable[] Variables { get; set; }
        }

        public class CounterExampleVariable
        {
            [XmlAttribute]
            public string Id { get; set; }

            [XmlAttribute]
            public int Value { get; set; }
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public CounterExampleType Status { get; set; }

        public string Error { get; set; }
        
    /*    public string CounterExampleType { get; set; }

        public BifurcatingVariableOutput[] BifurcatingVariables { get; set; }

        public OscillatingVariableOutput[] OscillatingVariables { get; set; }

        public string Error { get; set; }*/
    }

    [XmlRoot(ElementName = "AnalysisOutput")]
    public class CounterExampleOutputXML : CounterExampleOutput
    {
        //[XmlArrayItem("Variables")]
        [XmlElement("Variables", Type = typeof(CounterExampleVariables))]
        public CounterExampleVariables[] Variables { get; set; }

    }

    public class CycleCounterExample : CounterExampleOutput
    {
        public class CycleVariable
        {
            [XmlAttribute]
            public string Id { get; set; }

            [XmlAttribute]
            public int Value { get; set; }
        }

        public CycleVariable[] Variables { get; set; }
    }

    public class BifurcationCounterExample : CounterExampleOutput
    {
        public class BifurcatingVariable
        {
            [XmlAttribute]
            public string Id { get; set; }

            [XmlAttribute]
            public int Fix1 { get; set; }

            [XmlAttribute]
            public int Fix2 { get; set; }

        }

        public BifurcatingVariable[] Variables { get; set; }
    }


    public class FurtherTestingOutput
    {
        public CounterExampleOutput[] CounterExamples { get; set; }

        public string[] ErrorMessages { get; set; }

        public string[] DebugMessages { get; set; }
    }
}