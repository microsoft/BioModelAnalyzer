// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Xml.Serialization;

// SI: comments on these classes. 
// 1. Each subclass of CounterExampleOutput seems to define it's own Variable. Why not just define it once outside
//namespace BioModelAnalyzer
//{
//    public class CExVariable
//    {
//        public string Id { get; set; }
//        public int Value { get; set; }
//    }
//}
//    and use it in each subclass? 
//
// 2. BifurcationCounterExample should be defined like this:
//public class BifurcationCounterExample : CounterExampleOutput
//{

//    public CExVariable[] fix1 { get; set; }
//    public CExVariable[] fix2 { get; set; }
//}   
// to mimic the data that comes back from the F# solver. 


namespace BioModelAnalyzer
{
    public enum CounterExampleType
    {
        Bifurcation, Cycle, Fixpoint, Unknown
    }

    public class CounterExampleOutput    
    {
        public class Variable
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
        public Variable[] Variables { get; set; }
    }

    public class FixPointCounterExample : CounterExampleOutput
    {
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
