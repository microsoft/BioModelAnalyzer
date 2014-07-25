
using System.ServiceModel;
using System.ServiceModel.Activation;
using BioCheck.Web.Analysis;

namespace BioCheck.Web.Services
{
    [ServiceContract(Namespace = "")]
    [SilverlightFaultBehavior]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class AnalysisService
    {
        [OperationContract]
        public AnalysisOutputDTO Analyze(AnalysisInputDTO input)
        {
            var analyzer = new Analyzer();
            var output = analyzer.Analyze(input);
            return output;
        }

        [OperationContract]
        public FurtherTestingOutputDTO FindCounterExamples(AnalysisInputDTO input, AnalysisOutputDTO output)
        {
            var analyzer = new Analyzer();
            var cexoutput = analyzer.FindCounterExamples(input, output);
            return cexoutput;
        }

        [OperationContract]
        public SimulationOutputDTO Simulate(SimulationInputDTO input)
        {
            var analyzer = new Analyzer();
            var output = analyzer.Simulate(input);
            return output;
        }

        [OperationContract]
        public ValidationOutput Validate(string formmula)
        {
            var analyzer = new Analyzer();
            var validationOutput = analyzer.IsValid(formmula);
            return validationOutput;
        }
    }
}
