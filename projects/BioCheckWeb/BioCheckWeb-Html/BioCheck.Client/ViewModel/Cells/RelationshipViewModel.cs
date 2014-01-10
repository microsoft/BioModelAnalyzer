using System;
using BioCheck.Services;
using MvvmFx.Common.ViewModels.Behaviors.Observable;
using MvvmFx.Common.ViewModels.Commands;
using Microsoft.Practices.Unity;

namespace BioCheck.ViewModel.Cells
{
    public class RelationshipViewModel : ObservableViewModel, IDisposable
    {
        private VariableViewModel from;
        private VariableViewModel to;
        private bool isChecked;
        private ContainerViewModel containerViewModel;
        private readonly DelegateCommand deleteCommand;
        private RelationshipTypes type = RelationshipTypes.Activator;

        public RelationshipViewModel()
        {
            this.deleteCommand = new DelegateCommand(OnDeleteExecuted);
        }

        internal RelationshipViewModel Clone()
        {
            var clone = new RelationshipViewModel();
            clone.Id = this.Id;
            clone.isChecked = this.isChecked;
            clone.type = this.type;
            return clone;
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="From"/> property.
        /// </summary>
        public VariableViewModel From
        {
            get { return this.from; }
            set
            {
                if (this.from != value)
                {
                    this.from = value;
                    OnPropertyChanged(() => From);
                }
            }
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="To"/> property.
        /// </summary>
        public VariableViewModel To
        {
            get { return this.to; }
            set
            {
                if (this.to != value)
                {
                    this.to = value;
                    OnPropertyChanged(() => To);
                }
            }
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="IsChecked"/> property.
        /// </summary>
        public bool IsChecked
        {
            get { return this.isChecked; }
            set
            {
                if (this.isChecked != value)
                {
                    this.isChecked = value;
                    OnPropertyChanged(() => IsChecked);
                }
            }
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="ContainerViewModel"/> property.
        /// </summary>
        public ContainerViewModel ContainerViewModel
        {
            get { return this.containerViewModel; }
            set
            {
                if (this.containerViewModel != value)
                {
                    this.containerViewModel = value;
                    OnPropertyChanged(() => ContainerViewModel);
                }
            }
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="Type"/> property.
        /// </summary>
        public RelationshipTypes Type
        {
            get { return this.type; }
            set
            {
                if (this.type != value)
                {
                    this.type = value;
                    OnPropertyChanged(() => Type);
                }
            }
        }

        /// <summary>
        /// Gets the value of the <see cref="DeleteCommand"/> property.
        /// </summary>
        public DelegateCommand DeleteCommand
        {
            get { return this.deleteCommand; }
        }

        private void OnDeleteExecuted()
        {
            ApplicationViewModel.Instance.DupActiveModel();
            var modelVM = ApplicationViewModel.Instance.ActiveModel;
            modelVM.RelationshipViewModels.Remove(this);

            ApplicationViewModel.Instance.SaveActiveModel();
        }

        #region IDisposable Members

        private bool disposed;

        /// <summary>
        /// The Dispose method
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            // Use SupressFinalize in case a subclass
            // of this type implements a finalizer.
            //TODO GC.SuppressFinalize(this);
        }

        /// <summary>
        /// The protected virtual dispose that removes the handlers
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            // If you need thread safety, use a lock around these 
            // operations, as well as in your methods that use the resource.
            if (!disposed)
            {
                if (disposing)
                {
                    OnDispose();
                }

                // Indicate that the instance has been disposed.
                disposed = true;
            }
        }

        ~RelationshipViewModel()
        {
            Dispose(false);
        }

        protected virtual void OnDispose()
        {

        }

        #endregion
    }
}