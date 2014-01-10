using MvvmFx.Common.ViewModels;

namespace BioCheck.ViewModel.Simulation
{
    public class SimStepInfo : ViewModelBase
    {
        public string Name { get; set; }
        public int Value { get; set; }
        public bool HasChanged { get; set; }
        public bool HasCycled { get; set; }
        private bool isModified;

        public bool IsModified
        {
            get { return this.isModified; }
            set
            {
                if (this.isModified != value)
                {
                    this.isModified = value;
                    OnPropertyChanged(() => IsModified);
                }
            }
        }
    }
}