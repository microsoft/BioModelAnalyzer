using System.Xml.Serialization;
namespace bma.client
{
    public class SimulationVariable
    {
        public int Id { get; set; }

        public double Value { get; set; }
    }

    public class SimulationInput 
    {
        public Model Model { get; set; }

        public SimulationVariable[] Variables { get; set; }
    }

    public class SimulationOutput
    {
        public SimulationVariable[] Variables { get; set; }

        public string[] ErrorMessages { get; set; }
    }
}