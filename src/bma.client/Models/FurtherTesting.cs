using bma.client;
namespace bmaclient
{
    public class FurtherTestingInput 
    {
        public AnalysisInput Model { get; set; }

        public AnalysisOutput Analysis { get; set; }
    }

    public enum CounterExampleType
    {
        Bifurcation, Oscillation
    }

    public class BifurcatingVariableOutput
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string CalculatedBound { get; set; }

        public int Fix1 { get; set; }

        public int Fix2 { get; set; }
    }

    public class OscillatingVariableOutput
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string CalculatedBound { get; set; }

        public string Oscillation { get; set; }
    }

    public class CounterExampleOutput
    {
        public string Status { get; set; }

        public string CounterExampleType { get; set; }

        public BifurcatingVariableOutput[] BifurcatingVariables { get; set; }

        public OscillatingVariableOutput[] OscillatingVariables { get; set; }

        public string Error { get; set; }
    }

    public class FurtherTestingOutput
    {
        public CounterExampleOutput[] CounterExamples { get; set; }

        public string Error { get; set; }

        public string[] ErrorMessages { get; set; }
    }
}