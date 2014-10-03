using MvvmFx.Common.ViewModels;
using MvvmFx.Common.ViewModels.Commands;

namespace BioCheck.ViewModel.Cells
{
    public abstract class EditingViewModelBase : ViewModelBase
    {
        private readonly DelegateCommand hidePropertiesCommand;

        protected EditingViewModelBase()
        {

            this.hidePropertiesCommand = new DelegateCommand(OnHidePropertiesExecuted);

        }
        /// <summary>
        /// Gets the value of the <see cref="HidePropertiesCommand"/> property.
        /// </summary>
        public DelegateCommand HidePropertiesCommand
        {
            get { return this.hidePropertiesCommand; }
        }

        private void OnHidePropertiesExecuted()
        {
            ApplicationViewModel.Instance.ActiveVariable = null;
            ApplicationViewModel.Instance.ActiveContainer = null;
        }
    }
}