using System.Collections.Generic;

namespace BioCheck.ViewModel.Proof
{
    public class BifurcationCounterExample : CounterExampleInfo
    {
        public BifurcationCounterExample()
        {
            Type = CounterExampleTypes.Bifurcation;
        }

        public List<BifurcatingVariableInfo> VariableInfos { get; set; }
    }
}