using System.Xml.Serialization;
namespace bma.client
{
    public class AnalysisOutput : ModelAnalysis
    {
        public int Time { get; set; }

        public string[] ErrorMessages { get; set; }

        public string[] DebugMessages { get; set; }

        // DebugMessages and ErrorMessages go here
    }

    public class AnalysisInput : Model
    {
        [XmlAttribute]
        public string ModelName { get; set; }

        public string Engine { get; set; }

        [XmlIgnore]
        public bool EnableLogging { get; set; }
    }
}