using MvvmFx.Common.ViewModels;

namespace BioCheck.ViewModel.Proof
{
    public class VariableProofViewModel : ViewModelBase
    {
        private string targetFunction;
        private string cellname;
        private string name;
        private string range;

        /// <summary>
        /// Gets or sets the value of the <see cref="CellName"/> property.
        /// </summary>
        public string CellName
        {
            get { return this.cellname; }
            set
            {
                if (this.cellname != value)
                {
                    this.cellname = value;
                    OnPropertyChanged(() => CellName);
                }
            }
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="Name"/> property.
        /// </summary>
        public string Name
        {
            get { return this.name; }
            set
            {
                if (this.name != value)
                {
                    this.name = value;
                    OnPropertyChanged(() => Name);
                }
            }
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="TargetFunction"/> property.
        /// </summary>
        public string TargetFunction
        {
            get { return this.targetFunction; }
            set
            {
                if (this.targetFunction != value)
                {
                    this.targetFunction = value;
                    OnPropertyChanged(() => TargetFunction);
                }
            }
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="Range"/> property.
        /// </summary>
        public string Range
        {
            get { return this.range; }
            set
            {
                if (this.range != value)
                {
                    this.range = value;
                    OnPropertyChanged(() => Range);
                }
            }
        }
    }
}