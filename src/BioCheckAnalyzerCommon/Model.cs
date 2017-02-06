// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
using System;
using System.Linq;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace BioModelAnalyzer
{
    /// <summary>Model definition. This class serializes both to new JSON and old XML formats</summary>
    public class Model
    {
        public class Cell
        {
            [XmlAttribute]
            public string Name { get; set; }
        }

        public class Variable
        {
            [XmlAttribute]
            public int Id { get; set; }

            [DefaultValue((string)null)]
            [DisplayFormat(ConvertEmptyStringToNull = false)]
            public string Name { get; set; }

            public double RangeFrom { get; set; }

            public double RangeTo { get; set; }

            [DefaultValue((string)null)]
            [DisplayFormat(ConvertEmptyStringToNull = false)]
            public string Formula { get; set; }

            //public int? Number { get; set; }

            //public bool ShouldSerializeNumber()
            //{
            //    return Number != null;
            //}

            public Tag[] Tags { get; set; }
        }


        public class Tag
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }

        public class Relationship
        {
            [XmlAttribute]
            public int Id { get; set; }

            public int FromVariable { get; set; }

            public int ToVariable { get; set; }

            // [JsonConverter(typeof(StringEnumConverter))]
            // RelationshipType is encoded as number of JSON because attribute is commented
            public RelationshipType Type { get; set; }
        }

        public enum RelationshipType
        {
            Activator, Inhibitor
        }

        [XmlAttribute]
        public string ModelName { get; set; }

        public Cell[] Cells { get; set; }

        public Variable[] Variables { get; set; }

        public Relationship[] Relationships { get; set; }

    }
}
