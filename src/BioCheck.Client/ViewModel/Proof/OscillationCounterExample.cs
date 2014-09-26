using System.Collections.Generic;

namespace BioCheck.ViewModel.Proof
{
    public class OscillationCounterExample : CounterExampleInfo
    {
        public OscillationCounterExample()
        {
            Type = CounterExampleTypes.Oscillation;
        }

        public List<OscillatingVariableInfo> VariableInfos { get; set; }
    }
}