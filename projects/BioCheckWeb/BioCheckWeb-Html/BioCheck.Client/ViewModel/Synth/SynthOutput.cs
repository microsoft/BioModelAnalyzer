using System.Collections.Generic;
using System.Runtime.Serialization;
using BioCheck.AnalysisService;

namespace BioCheck.ViewModel.Proof
{
    //public class AnalysisTick
    //{
    //    public int Synth { get; set; }

    //    public List<VariableOutput> Variables { get; set; }
    //}

    public class SynthOutput
    {
        private readonly AnalysisOutputDTO dto;

        public SynthOutput(AnalysisOutputDTO dto)
        {
            this.dto = dto;         // All data.
            //this.Ticks = new List<AnalysisTick>();
            this.ErrorMessages = new List<string>();
        }

        public AnalysisOutputDTO Dto
        {
            get { return dto; }
        }

        public string Status { get; set; }

        public string Model { get; set; }

        // public List<AnalysisTick> Ticks { get; set; }

        public string Error { get; set; }

        public List<string> ErrorMessages { get; set; }

        public double Time { get; set; }

        public string Output { get; set; }
    }
}