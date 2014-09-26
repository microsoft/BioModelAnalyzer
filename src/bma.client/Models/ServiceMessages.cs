using System.Xml.Serialization;
namespace bma.client
{

    public class AnalysisOutput : ModelAnalysis
    {
        public int Time { get; set; }

        // DebugMessages and ErrorMessages go here
    }

    public class AnalysisInput : Model
    {
        [XmlAttribute]
        public string ModelName { get; set; }

        public string Engine { get; set; }
    }
}