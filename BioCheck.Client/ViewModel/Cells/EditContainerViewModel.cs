namespace BioCheck.ViewModel.Cells
{
    public class EditContainerViewModel : EditingViewModelBase
    {
        private readonly ContainerViewModel containerVM;

        public EditContainerViewModel(ContainerViewModel containerVM)
        {
            this.containerVM = containerVM;
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="Name"/> property.
        /// </summary>
        public string Name
        {
            get { return this.containerVM.Name; }
            set
            {
                if (this.containerVM.Name != value)
                {
                    this.containerVM.Name = value;
                    OnPropertyChanged(() => Name);
                }
            }
        }

    }
}