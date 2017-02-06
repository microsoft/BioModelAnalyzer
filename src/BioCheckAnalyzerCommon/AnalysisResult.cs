// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

// SI: comments on these classes. 
// 1. Why are Lo, Hi doubles? They should just be int? 

namespace BioModelAnalyzer
{
    public class Tick
    {
        public class Variable
        {
            [XmlAttribute]
            public int Id { get; set; }

            [XmlAttribute]
            public int Lo { get; set; }

            [XmlAttribute]
            public int Hi { get; set; }
        }

        public int Time { get; set; }

        [XmlArrayItem("Variable")]
        public Variable[] Variables { get; set; }
    }

    public class AnalysisResult
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public StatusType Status { get; set; }

        /// <summary>Additional error information if status is nor Stabilizing neither NonStabilizing</summary>
        [XmlIgnore]
        public string Error { get; set; }

        [XmlElement("Tick", Type = typeof(Tick))]
        public Tick[] Ticks { get; set; }
    }

    public class AnalysisResultDTO: AnalysisResult
    {
        [XmlElement("Loop", Type = typeof(int))]
        public int Loop { get; set; }

    }
    public class LTLAnalysisResultDTO
    {
        /// <summary>Additional error information if status is nor Stabilizing neither NonStabilizing</summary>
        [XmlIgnore]
        public string Error { get; set; }

        [XmlElement("Tick", Type = typeof(Tick))]
        public Tick[] Ticks { get; set; }

        public bool Status { get; set; }

        [XmlElement("Loop", Type = typeof(int))]
        public int Loop { get; set; }
    }

    public enum StatusType
    {
        Default,
        TryingStabilizing,
        Bifurcation,
        Cycle,
        Stabilizing,
        NotStabilizing,
        Fixpoint,
        Unknown,
        Error,
        True,
        False,
        PartiallyTrue
    }
}
