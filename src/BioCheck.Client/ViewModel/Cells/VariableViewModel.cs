using System;
using BioCheck.Services;
using BioCheck.ViewModel.Editing;
using BioCheck.ViewModel.Factories;
using MvvmFx.Common.ViewModels;
using MvvmFx.Common.ViewModels.Commands;
using MvvmFx.Common.ViewModels.Behaviors.Observable;
using Microsoft.Practices.Unity;

namespace BioCheck.ViewModel.Cells
{
    public class VariableViewModel : ObservableViewModel,
                                     ICopyable, IDisposable
    {
        private const int DefaultRangeFrom = 0;
        private const int DefaultRangeTo = 1;

        private string name;
        private ContainerViewModel containerViewModel;
        private bool isChecked;
        private int positionX;
        private int positionY;
        private int cellX;
        private int cellY;
        private VariableTypes type;
        private int rangeFrom;
        private int rangeTo;
        private string formula;
        private int? angle;

        private readonly DelegateCommand deleteCommand;
        private readonly ActionCommand cutCommand;
        private readonly ActionCommand copyCommand;
        private readonly ActionCommand pasteCommand;

        public VariableViewModel()
        {
            this.rangeFrom = DefaultRangeFrom;
            this.rangeTo = DefaultRangeTo;

            this.deleteCommand = new DelegateCommand(OnDeleteExecuted);
            this.cutCommand = new ActionCommand(arg => OnCutExecuted(), arg => this.Type != VariableTypes.MembraneReceptor);
            this.copyCommand = new ActionCommand(arg => OnCopyExecuted(), arg => this.Type != VariableTypes.MembraneReceptor);
            this.pasteCommand = new ActionCommand(arg => OnPasteExecuted(), arg => this.Type != VariableTypes.MembraneReceptor && CopyPasteManager.CanPaste(this));
        }

        internal VariableViewModel Clone()
        {
            var clone = new VariableViewModel();
            clone.Id = this.Id;
            clone.name = this.name;
            clone.isChecked = this.isChecked;
            clone.positionX = this.positionX;
            clone.positionY = this.positionY;
            clone.cellX = this.cellX;
            clone.cellY = this.cellY;
            clone.type = this.type;
            clone.rangeFrom = this.rangeFrom;
            clone.rangeTo = this.rangeTo;
            clone.formula = this.formula;
            clone.angle = this.angle;

            return clone;
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
        /// Gets or sets the value of the <see cref="Name"/> property.
        /// </summary>
        [Copy]
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
        /// Gets or sets the value of the <see cref="Formula"/> property.
        /// </summary>
        [Copy]
        public string Formula
        {
            get
            {
                return this.formula;
            }
            set
            {
                if (this.formula != value)
                {
                    this.formula = value;
                    OnPropertyChanged(() => Formula);
                }
            }
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="Type"/> property.
        /// </summary>
        [Copy]
        public VariableTypes Type
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
        /// Gets or sets the value of the <see cref="PositionX"/> property.
        /// </summary>
        [Copy]
        public int PositionX
        {
            get { return this.positionX; }
            set
            {
                if (this.positionX != value)
                {
                    this.positionX = value;
                    OnPropertyChanged(() => PositionX);
                }
            }
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="PositionY"/> property.
        /// </summary>
        [Copy]
        public int PositionY
        {
            get { return this.positionY; }
            set
            {
                if (this.positionY != value)
                {
                    this.positionY = value;
                    OnPropertyChanged(() => PositionY);
                }
            }
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="CellX"/> property.
        /// </summary>
        public int CellX
        {
            get { return this.cellX; }
            set
            {
                if (this.cellX != value)
                {
                    this.cellX = value;
                    OnPropertyChanged(() => CellX);
                }
            }
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="CellY"/> property.
        /// </summary>
        public int CellY
        {
            get { return this.cellY; }
            set
            {
                if (this.cellY != value)
                {
                    this.cellY = value;
                    OnPropertyChanged(() => CellY);
                }
            }
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="Angle"/> property.
        /// </summary>
        [Copy]
        public int? Angle
        {
            get { return this.angle; }
            set
            {
                if (this.angle != value)
                {
                    this.angle = value;
                    OnPropertyChanged(() => Angle);
                }
            }
        }

        /// <summary>
        /// Gets the left in grid cells
        /// </summary>
        public int Left
        {
            get { return (this.cellX * ApplicationSettings.CellWidth) + this.positionX; }
        }

        /// <summary>
        /// Gets the top in grid cells
        /// </summary>
        public int Top
        {
            get { return (this.cellY * ApplicationSettings.CellHeight) + this.positionY; }
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

        private bool isStable;

        /// <summary>
        /// Gets or sets the value of the <see cref="IsStable"/> property.
        /// </summary>
        public bool IsStable
        {
            get { return this.isStable; }
            set
            {
                if (this.isStable != value)
                {
                    this.isStable = value;
                    OnPropertyChanged(() => IsStable);
                }
            }
        }

        private string stabilityValue;

        /// <summary>
        /// Gets or sets the value of the <see cref="StabilityValue"/> property.
        /// </summary>
        public string StabilityValue
        {
            get { return this.stabilityValue; }
            set
            {
                if (this.stabilityValue != value)
                {
                    this.stabilityValue = value;
                    OnPropertyChanged(() => StabilityValue);
                }
            }
        }

        private bool showStability;

        /// <summary>
        /// Gets or sets the value of the <see cref="ShowStability"/> property.
        /// </summary>
        public bool ShowStability
        {
            get { return this.showStability; }
            set
            {
                if (this.showStability != value)
                {
                    this.showStability = value;
                    OnPropertyChanged(() => ShowStability);
                }
            }
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="RangeFrom"/> property.
        /// </summary>
        [Copy]
        public int RangeFrom
        {
            get { return this.rangeFrom; }
            set
            {
                if (this.rangeFrom != value)
                {
                    this.rangeFrom = value;
                    OnPropertyChanged(() => RangeFrom);

                    // Reset the Stability
                    ResetStability();
                }
            }
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="RangeTo"/> property.
        /// </summary>
        [Copy]
        public int RangeTo
        {
            get { return this.rangeTo; }
            set
            {
                if (this.rangeTo != value)
                {
                    this.rangeTo = value;
                    OnPropertyChanged(() => RangeTo);

                    // Reset the Stability
                    ResetStability();
                }
            }
        }

        public void ResetStability()
        {
            this.ShowStability = false;
            this.StabilityValue = string.Empty;

            if (this.containerViewModel != null)
                this.containerViewModel.ShowStability = false;
        }

        #region Commands

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
            if (this.containerViewModel != null)
            {
                this.containerViewModel.VariableViewModels.Remove(this);
            }
            else
            {
                var modelVM = ApplicationViewModel.Instance.ActiveModel;
                modelVM.VariableViewModels.Remove(this);
            }
            ApplicationViewModel.Instance.SaveActiveModel();
        }

        /// <summary>
        /// Gets the value of the <see cref="CutCommand"/> property.
        /// </summary>
        public ActionCommand CutCommand
        {
            get { return this.cutCommand; }
        }

        private void OnCutExecuted()
        {
            OnCopyExecuted();
            OnDeleteExecuted();
        }

        /// <summary>
        /// Gets the value of the <see cref="CopyCommand"/> property.
        /// </summary>
        public ActionCommand CopyCommand
        {
            get { return this.copyCommand; }
        }

        private void OnCopyExecuted()
        {
            CopyPasteManager.Copy(this);
        }

        /// <summary>
        /// Gets the value of the <see cref="PasteCommand"/> property.
        /// </summary>
        public ActionCommand PasteCommand
        {
            get { return this.pasteCommand; }
        }

        private void OnPasteExecuted()
        {
            CopyPasteManager.Paste(this);
        }
    
        #endregion

        #region Copy/Paste

        public ViewModelBase Copy()
        {
            var modelVM = ApplicationViewModel.Instance.ActiveModel;

            var variableVMCopy = new VariableViewModel();
            variableVMCopy.Id = IdFactory.NewVariableId(modelVM);

            CopyPasteManager.Paste(this, variableVMCopy);

            return variableVMCopy;
        }

        public bool CanPaste(ViewModelBase source)
        {
            var variableVMCopy = source as VariableViewModel;

            if (variableVMCopy != null &&
                variableVMCopy.Type == type)
            {
                return true;
            }

            return false;
        }

        public void Paste(ViewModelBase source)
        {
            // Allow a variable to have its values pasted over the top of another variable.
            if (this.CanPaste(source))
            {
                var oldX = this.PositionX;
                var oldY = this.PositionY;

                CopyPasteManager.Paste(source, this);

                this.PositionX = oldX;
                this.PositionY = oldY;
            }
        }

        #endregion

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

        ~VariableViewModel()
        {
            Dispose(false);
        }

        protected virtual void OnDispose()
        {

        }

        #endregion
    }
}