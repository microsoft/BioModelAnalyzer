using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace bma.client { 

    public class Model
    {
        public Cell[] Cells { get; set; }
        public Variable[] Variables { get; set; }

        public Relationship[] Relationships { get; set; }
    }

    public class Cell
    {
        [XmlAttribute]
        public string Name { get; set; }
    }

    public class Variable
    {
        [XmlAttribute]
        public int Id { get; set; }

        public string Name { get; set; }

        public double RangeFrom { get; set; }

        public double RangeTo { get; set; }

        [DefaultValue((string)null)]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string Function { get; set; }

        public int? Number { get; set; }

        public bool ShouldSerializeNumber()
        {
            return Number != null;
        }

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

        public int FromVariableId { get; set; }

        public int ToVariableId { get; set; }

        public RelationshipType Type { get; set; }
    }

    public enum RelationshipType
    {
        Activator, Inhibitor
    }
}