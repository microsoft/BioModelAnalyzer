using System.Collections.Generic;

namespace BioCheck.ViewModel.Proof
{
    public class ProgressionInfo
    {
        public ProgressionInfo()
        {
            Steps = new List<ProgressionStepInfo>();
        }
        public int Id { get; set; }

        public string CellName { get; set; }

        public string Name { get; set; }

        public List<ProgressionStepInfo> Steps { get; set; }
    }
}