using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Xml.Serialization;

namespace BioModelAnalyzer
{
    public enum CounterExampleType
    {
        Bifurcation, Cycle, Fixpoint, Unknown
    }

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
    }

    public class CycleCounterExample : CounterExampleOutput
    {
        public class Variable
        {
            [XmlAttribute]
            public string Id { get; set; }

            [XmlAttribute]
            public int Value { get; set; }
        }

        public Variable[] Variables { get; set; }
    }

    public class FixPointCounterExample : CounterExampleOutput
    {
        public class Variable
        {
            [XmlAttribute]
            public string Id { get; set; }

            [XmlAttribute]
            public int Value { get; set; }
        }

        public Variable[] Variables { get; set; }
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
}