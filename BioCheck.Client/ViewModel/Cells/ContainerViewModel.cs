using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BioCheck.Services;
using BioCheck.ViewModel.Editing;
using BioCheck.ViewModel.Factories;
using Microsoft.Practices.ObjectBuilder2;
using MvvmFx.Common.Helpers;
using MvvmFx.Common.ViewModels;
using MvvmFx.Common.ViewModels.Commands;
using MvvmFx.Common.ViewModels.Behaviors.Observable;
using Microsoft.Practices.Unity;

namespace BioCheck.ViewModel.Cells
{
    public class ContainerViewModel : ObservableViewModel,
                                      ICopyable,
                                      IDisposable
    {
        private string name;
        private readonly ViewModelCollection<VariableViewModel> variableViewModels;
        private bool isChecked;
        private bool isStable;
        private bool showStability;
        private int positionX;
        private int positionY;

        private readonly DelegateCommand deleteCommand;
        private readonly DelegateCommand cutCommand;
        private readonly DelegateCommand copyCommand;
        private readonly ActionCommand pasteCommand;
        private readonly DelegateCommand moveCommand;

        public ContainerViewModel()
        {
            this.variableViewModels = new ViewModelCollection<VariableViewModel>();

            this.variableViewModels.ItemRemoved += variableViewModels_ItemRemoved;

            this.deleteCommand = new DelegateCommand(OnDeleteExecuted);
            this.cutCommand = new DelegateCommand(OnCutExecuted);
            this.copyCommand = new DelegateCommand(OnCopyExecuted);
            this.pasteCommand = new ActionCommand(arg => OnPasteExecuted(), arg => CopyPasteManager.CanPaste(this));
            this.moveCommand = new DelegateCommand(OnMoveExecuted);
            this.resizeCommand = new ActionCommand(OnResizeExecuted);

            this.size = ContainerSizeTypes.One;
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
                    OnPropertyChanged(() => HasName);
                }
            }
        }

        /// <summary>
        /// Gets whether the Container has a Name 
        /// </summary>
        public bool HasName
        {
            get { return !string.IsNullOrEmpty(this.name); }
        }

        /// <summary>
        /// Gets the value of the <see cref="VariableViewModels"/> property.
        /// </summary>
        public ViewModelCollection<VariableViewModel> VariableViewModels
        {
            get { return variableViewModels; }
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="PositionX"/> property.
        /// </summary>
        public int PositionX
        {
            get { return this.positionX; }
            set
            {
                if (this.positionX != value)
                {
                    this.positionX = value;
                    OnPropertyChanged(() => PositionX);

                    OnPropertyChanged(() => Left);
                }
            }
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="PositionY"/> property.
        /// </summary>
        public int PositionY
        {
            get { return this.positionY; }
            set
            {
                if (this.positionY != value)
                {
                    this.positionY = value;
                    OnPropertyChanged(() => PositionY);

                    OnPropertyChanged(() => Top);
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
        /// Creates a new variable in the container
        /// </summary>
        /// <param name="positionX">The position X.</param>
        /// <param name="positionY">The position Y.</param>
        /// <returns></returns>
        public VariableViewModel NewVariable(double positionX, double positionY)
        {
            return NewVariable(positionX, positionY, VariableTypes.Default);
        }

        public VariableViewModel NewVariable(double positionX, double positionY, VariableTypes variableType)
        {
            var variableVM = new VariableViewModel();

            variableVM.Id = IdFactory.NewVariableId(ApplicationViewModel.Instance.ActiveModel);
            variableVM.ContainerViewModel = this;

            variableVM.Type = variableType;

            var intX = Convert.ToInt32(positionX);
            var intY = Convert.ToInt32(positionY);

            bool notEmpty = true;
            while (notEmpty)
            {
                // Check if there is already a variable at this position
                notEmpty = (from vvm in this.variableViewModels
                            where vvm.PositionX == intX && vvm.PositionY == intY
                            select vvm).Any();

                if (notEmpty)
                {
                    intX += 5;
                    intY += 5;
                }
            }

            variableVM.PositionX = intX;
            variableVM.PositionY = intY;
            variableVM.CellX = this.PositionX;
            variableVM.CellY = this.PositionY;

            this.variableViewModels.Add(variableVM);

            return variableVM;
        }

        /// <summary>
        /// Creates a new membrane receptor.
        /// </summary>
        public void NewMembraneReceptor(double positionX, double positionY, int angle)
        {
            var variableVM = new VariableViewModel();

            variableVM.Id = IdFactory.NewVariableId(ApplicationViewModel.Instance.ActiveModel);
            variableVM.ContainerViewModel = this;
            variableVM.PositionX = Convert.ToInt32(positionX);
            variableVM.PositionY = Convert.ToInt32(positionY);
            variableVM.CellX = this.PositionX;
            variableVM.CellY = this.PositionY;
            variableVM.Angle = angle;

            // Set the type to MembraneReceptor
            variableVM.Type = VariableTypes.MembraneReceptor;

            this.variableViewModels.Add(variableVM);
        }

        /// <summary>
        /// Handles the ItemRemoved event of the variableViewModels control and removes the
        /// any corresponding relationships.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MvvmFx.Common.ViewModels.ItemEventArgs&lt;BioCheck.ViewModel.Cells.VariableViewModel&gt;"/> instance containing the event data.</param>
        void variableViewModels_ItemRemoved(object sender, ItemEventArgs<VariableViewModel> e)
        {
            var variableVM = e.Item;
            var activeModel = ApplicationViewModel.Instance.ActiveModel;

            // Delete any relationships this variable was involved in
            activeModel.RelationshipViewModels.RemoveAll(rvm => rvm.From == variableVM || rvm.To == variableVM);
        }

        /// <summary>
        /// Resets the membrane receptors.
        /// </summary>
        public void ResetMembraneReceptors()
        {
            var modelVM = ApplicationViewModel.Instance.ActiveModel;

            var variablesToReset = (from vvm in this.variableViewModels
                                    where vvm.Type == VariableTypes.MembraneReceptor
                                    select vvm)
                 .ToList();

            var relationshipsToReset = new List<RelationshipViewModel>();

            variablesToReset.ForEach(vvm =>
                                         {
                                             // Get the relationships it's involved in
                                             var relationshipVMs = (from rvm in modelVM.RelationshipViewModels
                                                                    where rvm.From == vvm || rvm.To == vvm
                                                                    select rvm)
                                                                   .ToList();

                                             relationshipsToReset.AddRange(relationshipVMs);
                                         });

            // Re-draw the relationships
            relationshipsToReset.Distinct().ForEach(rvm => modelVM.RelationshipViewModels.Remove(rvm));
            relationshipsToReset.Distinct().ForEach(rvm => modelVM.RelationshipViewModels.Add(rvm));
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
            // Prompt the user and delete the container if they confirm
            ApplicationViewModel.Instance.Container
                .Resolve<BioCheck.Services.IMessageWindowService>()
                .Show(
                    "Are you sure you want to delete the current container?",
                    MessageType.YesCancel, result =>
                                               {
                                                   if (result == MessageResult.Yes)
                                                   {
                                                       this.VariableViewModels.Clear();

                                                       var modelVM = ApplicationViewModel.Instance.ActiveModel;
                                                       modelVM.RelationshipViewModels.RemoveAll(
                                                           rvm => rvm.From.ContainerViewModel == this);
                                                       modelVM.RelationshipViewModels.RemoveAll(
                                                           rvm => rvm.To.ContainerViewModel == this);
                                                       modelVM.ContainerViewModels.Remove(this);
                                                   }
                                               });
        }

        /// <summary>
        /// Gets the value of the <see cref="CutCommand"/> property.
        /// </summary>
        public DelegateCommand CutCommand
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
        public DelegateCommand CopyCommand
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

            var containerVMCopy = new ContainerViewModel();

            // Get the current largest container id
            containerVMCopy.Id = IdFactory.NewContainerId(modelVM);

            CopyPasteManager.Paste(this, containerVMCopy);

            var variableCopies = new Dictionary<VariableViewModel, VariableViewModel>();

            // Get the current largest variable id
            // to use as the new seed number for variable id's that haven't been set yet
            int variableId = IdFactory.NewVariableId(modelVM);

            foreach (var variableVM in this.VariableViewModels)
            {
                var variableVMCopy = containerVMCopy.NewVariable(variableVM.PositionX, variableVM.PositionY);
                variableVMCopy.Id = variableId++;

                CopyPasteManager.Paste(variableVM, variableVMCopy);

                variableCopies.Add(variableVM, variableVMCopy);
            }

            // Get the current largest relationship id
            int relationshipId = IdFactory.NewRelationshipId(modelVM);

            var relationshipVMs = modelVM.RelationshipViewModels.Where(rvm => rvm.From.ContainerViewModel == this && rvm.To.ContainerViewModel == this)
                .ToList();

            var copiedRelationships = new List<RelationshipViewModel>();

            foreach (var relationshipVM in relationshipVMs)
            {
                var variableCopyFrom = variableCopies[relationshipVM.From];
                var variableCopyTo = variableCopies[relationshipVM.To];

                // Create a new relationship but store it in the clipboard
                var relationshipVMCopy = modelVM.NewRelationship(variableCopyFrom, variableCopyTo, relationshipVM.Type);
                copiedRelationships.Add(relationshipVMCopy);
                modelVM.RelationshipViewModels.Remove(relationshipVMCopy);

                relationshipVMCopy.Id = relationshipId++;
            }

            CopyPasteManager.ClipboardData["Relationships"] = copiedRelationships;

            return containerVMCopy;
        }

        public bool CanPaste(ViewModelBase source)
        {
            if (source is ContainerViewModel)
            {
                return true;
            }

            // Only allow Default variables to be pasted into containers
            var variableVMCopy = source as VariableViewModel;

            if (variableVMCopy != null &&
                variableVMCopy.Type == VariableTypes.Default)
            {
                return true;
            }

            return false;
        }

        public void Paste(ViewModelBase source)
        {
            if (source is ContainerViewModel)
            {
                this.Paste((ContainerViewModel)source);
            }
            else if (source is VariableViewModel)
            {
                this.Paste((VariableViewModel)source);
            }
        }

        private void Paste(ContainerViewModel sourceContainerVM)
        {
            CopyPasteManager.Paste(sourceContainerVM, this);

            this.VariableViewModels.RemoveAll();

            var variableCopies = new Dictionary<VariableViewModel, VariableViewModel>();

            foreach (var variableVM in sourceContainerVM.VariableViewModels)
            {
                var newVariableVM = this.NewVariable(variableVM.PositionX, variableVM.PositionY, variableVM.Type);
                newVariableVM.Paste(variableVM);

                variableCopies.Add(variableVM, newVariableVM);
            }

            // Remove the relationships in this container
            var modelVM = ApplicationViewModel.Instance.ActiveModel;
            modelVM.RelationshipViewModels.RemoveAll(rvm => rvm.From.ContainerViewModel == this && rvm.To.ContainerViewModel == this);

            // Get the relationships in the clipboard
            var copiedRelationships = CopyPasteManager.ClipboardData["Relationships"] as List<RelationshipViewModel>;

            if (copiedRelationships != null)
            {
                foreach (var relationshipVM in copiedRelationships)
                {
                    var variableCopyFrom = variableCopies[relationshipVM.From];
                    var variableCopyTo = variableCopies[relationshipVM.To];

                    var relationshipVMCopy = modelVM.NewRelationship(variableCopyFrom, variableCopyTo, relationshipVM.Type);
                    relationshipVMCopy.Id = relationshipVM.Id;
                }
            }
        }

        private void Paste(VariableViewModel variableVMCopy)
        {
            // Only allow Default variables to be pasted into containers
            if (variableVMCopy.Type == VariableTypes.Default)
            {
                var newVariableVM = this.NewVariable(variableVMCopy.PositionX, variableVMCopy.PositionY);
                newVariableVM.Paste(variableVMCopy);

                // Offset the positions slightly 
                newVariableVM.PositionX += 10;
                newVariableVM.PositionY += 10;
            }
        }

        /// <summary>
        /// Gets the value of the <see cref="MoveCommand"/> property.
        /// </summary>
        public DelegateCommand MoveCommand
        {
            get { return this.moveCommand; }
        }

        private void OnMoveExecuted()
        {

            //  this.dragDropService = new DragDropService<ContainerViewModel>(mouseContext.ContainerView, dragOrigin);



        }

        #endregion

        #region Size

        private readonly ActionCommand resizeCommand;

        /// <summary>
        /// Gets the value of the <see cref="ResizeCommand"/> property.
        /// </summary>
        public ActionCommand ResizeCommand
        {
            get { return this.resizeCommand; }
        }

        private void OnResizeExecuted(object param)
        {
            var oldSize = this.size;
            var newSize = EnumHelper.Parse<ContainerSizeTypes>(param.ToString());

            var modelVM = ApplicationViewModel.Instance.ActiveModel;

            if (newSize > oldSize)
            {
                // E.g. 1->2 or 1->3 or 2->3
                var delta = newSize - oldSize;

                // Increment the number of rows and columns
                modelVM.Rows += delta;
                modelVM.Columns += delta;

                var variablesToReset = new List<VariableViewModel>();
                var variablesToResetAndMove = new List<VariableViewModel>();
                var relationshipsToReset = new List<RelationshipViewModel>();

                variablesToResetAndMove.AddRange(this.VariableViewModels);


                var getContainerBottomY = new Func<ContainerViewModel, int>(cvm =>
                                                                                {
                                                                                    if (cvm.SizeOne)
                                                                                        return cvm.PositionY;

                                                                                    if (cvm.SizeTwo)
                                                                                        return cvm.PositionY + 1;

                                                                                    if (cvm.SizeThree)
                                                                                        return cvm.PositionY + 2;

                                                                                    return cvm.PositionY;
                                                                                });

                var getContainerRightX = new Func<ContainerViewModel, int>(cvm =>
                {
                    if (cvm.SizeOne)
                        return cvm.PositionX;

                    if (cvm.SizeTwo)
                        return cvm.PositionX + 1;

                    if (cvm.SizeThree)
                        return cvm.PositionX + 2;

                    return cvm.PositionX;
                });

                // Move every container to the right or below it into the next row/column.
                foreach (var containerVM in modelVM.ContainerViewModels)
                {
                    bool isAffected = false;


                    // TODO - if one cell on the x axis has to move, then all the others passed it should too, else the first one that gets moved could overlap the others.
                    // TODO - check constants can't be overlapped by resizing cells.


                    if (containerVM.PositionX > this.positionX && getContainerBottomY(containerVM) >= this.positionY)
                    {
                        containerVM.PositionX += delta;
                        isAffected = true;
                    }

                    if (containerVM.PositionY > this.positionY && getContainerRightX(containerVM) >= this.positionX)
                    {
                        isAffected = true;
                        containerVM.PositionY += delta;
                    }

                    if (isAffected)
                    {
                        containerVM.VariableViewModels.ForEach(vvm =>
                                                                   {
                                                                       // update the variable's cell position, so that the relationships are drawn correctly
                                                                       vvm.CellX = containerVM.PositionX;
                                                                       vvm.CellY = containerVM.PositionY;
                                                                       variablesToReset.Add(vvm);
                                                                   });
                    }
                }

                var affectedConstants = new List<VariableViewModel>();

                foreach (var variableVM in modelVM.VariableViewModels)
                {
                    if (variableVM.Type == VariableTypes.Constant)
                    {
                        bool isAffected = (variableVM.CellX > this.positionX && variableVM.CellY >= this.positionY) ||
                                          (variableVM.CellY > this.positionY && variableVM.CellX >= this.positionX);

                        if (isAffected)
                            affectedConstants.Add(variableVM);
                    }
                }

                this.Size = newSize;

                // Update the affected variables by removing and re-adding them
                variablesToResetAndMove.ForEach(variableVM =>
                                              {
                                                  // Get the relationships it's involved in
                                                  var relationshipVMs = (from rvm in modelVM.RelationshipViewModels
                                                                         where rvm.From == variableVM || rvm.To == variableVM
                                                                         select rvm)
                                                                        .ToList();

                                                  relationshipsToReset.AddRange(relationshipVMs);

                                                  // Remove the constant from the variables
                                                  // This will remove it from its current ContainerSite
                                                  var sourceContainer = variableVM.ContainerViewModel;
                                                  sourceContainer.VariableViewModels.Remove(variableVM);

                                                  // Change its Cell and Position
                                                  variableVM.PositionX += ((ApplicationSettings.CellWidth * delta) / 2);
                                                  variableVM.PositionY += ((ApplicationSettings.CellHeight * delta) / 2);

                                                  // Re-add it 
                                                  sourceContainer.VariableViewModels.Add(variableVM);
                                              });

                variablesToReset.ForEach(variableVM =>
                {
                    // Get the relationships it's involved in
                    var relationshipVMs = (from rvm in modelVM.RelationshipViewModels
                                           where rvm.From == variableVM || rvm.To == variableVM
                                           select rvm)
                                          .ToList();

                    relationshipsToReset.AddRange(relationshipVMs);

                    // Remove the constant from the variables
                    // This will remove it from its current ContainerSite
                    var sourceContainer = variableVM.ContainerViewModel;
                    sourceContainer.VariableViewModels.Remove(variableVM);

                    // Re-add it 
                    sourceContainer.VariableViewModels.Add(variableVM);
                });


                // Update the affected constants by removing and re-adding them
                affectedConstants.ForEach(constantVM =>
                                              {
                                                  // Get the relationships it's involved in
                                                  var relationshipVMs =
                                                      modelVM.RelationshipViewModels.Where(
                                                          rvm => rvm.From == constantVM || rvm.To == constantVM)
                                                          .ToList();

                                                  relationshipsToReset.AddRange(relationshipVMs);

                                                  // Remove the constant from the variables
                                                  // This will remove it from its current ContainerSite
                                                  modelVM.VariableViewModels.Remove(constantVM);

                                                  // Update the CellX and CellY properties
                                                  // Note: if we do this before removing the variable, it will not be found and removed in ContainerGrid
                                                  if (constantVM.CellX > this.positionX && constantVM.CellY >= this.positionY)
                                                  {
                                                      constantVM.CellX += delta;
                                                  }

                                                  if (constantVM.CellY > this.positionY && constantVM.CellX >= this.positionX)
                                                  {
                                                      constantVM.CellY += delta;
                                                  }

                                                  // Re-add it 
                                                  modelVM.VariableViewModels.Add(constantVM);
                                              });


                // Re-draw the relationships
                relationshipsToReset.Distinct().ForEach(rvm => modelVM.RelationshipViewModels.Add(rvm));

                // TODO - use the move tool?

            }
            else if (newSize < oldSize)
            {
                // E.g. 2->1 or 3->1 or 3.>2
                var delta = oldSize - newSize;

                var variablesToReset = new List<VariableViewModel>();
                var variablesToResetAndMove = new List<VariableViewModel>();
                var relationshipsToReset = new List<RelationshipViewModel>();

                variablesToResetAndMove.AddRange(this.VariableViewModels);

                // Need to worry about the variables if shrinking the size
                this.Size = newSize;

                // Update the affected variables by removing and re-adding them
                variablesToResetAndMove.ForEach(variableVM =>
                {
                    // Get the relationships it's involved in
                    var relationshipVMs = (from rvm in modelVM.RelationshipViewModels
                                           where rvm.From == variableVM || rvm.To == variableVM
                                           select rvm)
                                          .ToList();

                    relationshipsToReset.AddRange(relationshipVMs);

                    // Remove the constant from the variables
                    // This will remove it from its current ContainerSite
                    var sourceContainer = variableVM.ContainerViewModel;
                    sourceContainer.VariableViewModels.Remove(variableVM);

                    // Change its Cell and Position
                    variableVM.PositionX -= ((ApplicationSettings.CellWidth * delta) / 2);
                    variableVM.PositionY -= ((ApplicationSettings.CellHeight * delta) / 2);

                    // Re-add it 
                    sourceContainer.VariableViewModels.Add(variableVM);
                });

                // Re-draw the relationships
                relationshipsToReset.Distinct().ForEach(rvm => modelVM.RelationshipViewModels.Add(rvm));
            }
        }

        private ContainerSizeTypes size;

        /// <summary>
        /// Gets or sets the value of the <see cref="Size"/> property.
        /// </summary>
        [Copy]
        public ContainerSizeTypes Size
        {
            get { return this.size; }
            set
            {
                if (this.size != value)
                {
                    this.size = value;
                    OnPropertyChanged(() => Size);
                    OnPropertyChanged(() => SizeOne);
                    OnPropertyChanged(() => SizeTwo);
                    OnPropertyChanged(() => SizeThree);
                    OnPropertyChanged(() => Width);
                    OnPropertyChanged(() => Height);
                }
            }
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="SizeOne"/> property.
        /// </summary>
        public bool SizeOne
        {
            get { return this.size == ContainerSizeTypes.One; }
            set
            {
                if (value)
                {
                    this.size = ContainerSizeTypes.One;
                    OnPropertyChanged(() => SizeOne);
                    OnPropertyChanged(() => SizeTwo);
                    OnPropertyChanged(() => SizeThree);
                }
            }
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="SizeTwo"/> property.
        /// </summary>
        public bool SizeTwo
        {
            get { return this.size == ContainerSizeTypes.Two; }
            set
            {
                if (value)
                {
                    this.size = ContainerSizeTypes.Two;
                    OnPropertyChanged(() => SizeOne);
                    OnPropertyChanged(() => SizeTwo);
                    OnPropertyChanged(() => SizeThree);
                }
            }
        }
        /// <summary>
        /// Gets or sets the value of the <see cref="SizeThree"/> property.
        /// </summary>
        public bool SizeThree
        {
            get { return this.size == ContainerSizeTypes.Three; }
            set
            {
                if (value)
                {
                    this.size = ContainerSizeTypes.Three;
                    OnPropertyChanged(() => SizeOne);
                    OnPropertyChanged(() => SizeTwo);
                    OnPropertyChanged(() => SizeThree);
                }
            }
        }

        /// <summary>
        /// Gets the width in grid cells
        /// </summary>
        public int Width
        {
            get { return ApplicationSettings.CellWidth * (int)this.size; }
        }

        /// <summary>
        /// Gets the height in grid cells
        /// </summary>
        public int Height
        {
            get { return ApplicationSettings.CellHeight * (int)this.size; }
        }

        /// <summary>
        /// Gets the left in grid cells
        /// </summary>
        public int Left
        {
            get { return this.positionX * ApplicationSettings.CellWidth; }
        }

        /// <summary>
        /// Gets the top in grid cells
        /// </summary>
        public int Top
        {
            get { return this.positionY * ApplicationSettings.CellHeight; }
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

        ~ContainerViewModel()
        {
            Dispose(false);
        }

        protected virtual void OnDispose()
        {

        }

        #endregion
    }
}