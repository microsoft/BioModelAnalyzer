using System.Xml.Serialization;

namespace bma.client { 
   

    public class ModelAnalysis
    {
        public class Tick
        {
            public class Variable {
                [XmlAttribute]
                public int Id { get; set; }

                [XmlAttribute]
                public double Lo { get; set; }

                [XmlAttribute]
                public double Hi { get; set; }
            }

            public int Time { get; set; }

            [XmlArrayItem("Variable")]
            public Variable[] Variables { get; set; }
        }

        public StatusType Status { get; set; }

        [XmlElement("Tick", Type = typeof(Tick))]
        public Tick[] Ticks { get; set; }
    }

}