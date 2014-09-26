using System.Collections.Generic;
using System.Runtime.Serialization;

namespace BioCheck.Web.Analysis
{
    public class AnalysisInputDTO
    {
        [DataMember()]
        public string ModelName { get; set; }

        [DataMember()]
        public bool EnableLogging { get; set; }

        [DataMember()]
        public byte[] ZippedXml { get; set; }
    }

    public class SimVariableDTO
    {
        public int Id { get; set; }
        public int Value { get; set; }

        public override string ToString()
        {
            return string.Format("{0} : {1}", Id, Value);
        }
    }

    public class SimulationOutputDTO
    {
        [DataMember()]
        public List<SimVariableDTO> Variables { get; set; }

        public string Error { get; set; }

        public List<string> ErrorMessages { get; set; }

        [DataMember()]
        public byte[] ZippedLog { get; set; }
    }

    public class SimulationInputDTO
    {
        [DataMember()]
        public string ModelName { get; set; }

        [DataMember()]
        public List<SimVariableDTO> Variables { get; set; }

        [DataMember()]
        public bool EnableLogging { get; set; }

        [DataMember()]
        public byte[] ZippedXml { get; set; }
    }


    public class AnalysisInput
    {
        public AnalysisInput()
        {

        }

        [DataMember()]
        public string ModelName { get; set; }

        [DataMember()]
        public bool EnableLogging { get; set; }

        [DataMember()]
        public List<VariableInput> Variables { get; set; }

        [DataMember()]
        public List<RelationshipInput> Relationships { get; set; }
    }
}