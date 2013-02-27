using System;
using System.IO.IsolatedStorage;
using System.Linq;
using BioCheck.ViewModel.Cells;
using BioCheck.ViewModel.Factories;
using MvvmFx.Common.Helpers;
using MvvmFx.Common.ViewModels;
using MvvmFx.Common.ViewModels.Behaviors.LoadingSaving;
using MvvmFx.Common.ViewModels.Behaviors.Messaging;
using MvvmFx.Common.ViewModels.Behaviors.Observable;
using Microsoft.Practices.Unity;

namespace BioCheck.ViewModel.Models
{
    public class ModelViewModel : ObservableViewModel
    {
        private const int DefaultRows = 5;
        private const int DefaultColumns = 6;

        private readonly ContainerViewModels containerViewModels;
        private readonly MappedProperty mappedZoomLevel;
        private readonly ViewModelCollection<RelationshipViewModel> relationshipViewModels;
        private readonly ViewModelCollection<VariableViewModel> variableViewModels;
        private bool showStability;

        private string name;
        private string displayName;
        private int versions = 1;
        private const string DateFormatString = "MM.dd.yy";
        private readonly string version = "Version 1";
        private DateTime createdDate;
        private DateTime modifiedDate;
        private string description;
        private string author;
        private double panX;
        private double panY;
        private int rows;
        private int columns;
        private bool isLoaded;

        public ModelViewModel()
        {
            this.containerViewModels = new ContainerViewModels();
            this.containerViewModels.ItemRemoved += containerViewModels_ItemRemoved;

            this.variableViewModels = new ViewModelCollection<VariableViewModel>();
            this.variableViewModels.ItemRemoved += variableViewModels_ItemRemoved;

            this.relationshipViewModels = new ViewModelCollection<RelationshipViewModel>();
            this.relationshipViewModels.ItemRemoved += relationshipViewModels_ItemRemoved;

            // Map the ZoomLevel property that is registered by the ZoomViewModel
            this.mappedZoomLevel = this.Messenger.ResolveProperty(this, () => ZoomLevel);
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="IsLoaded"/> property.
        /// </summary>
        public bool IsLoaded
        {
            get { return this.isLoaded; }
            set
            {
                if (this.isLoaded != value)
                {
                    this.isLoaded = value;
                    OnPropertyChanged(() => IsLoaded);
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
                    this.displayName = value;

                    OnPropertyChanged(() => Name);
                }
            }
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="DisplayName"/> property.
        /// </summary>
        public string DisplayName
        {
            get { return this.displayName; }
            set
            {
                if (this.displayName != value)
                {
                    this.displayName = value;

                    OnDisplayNameChanging();

                    OnPropertyChanged(() => DisplayName);
                }
            }
        }

        private void OnDisplayNameChanging()
        {
            // TODO - assumes offline mode
            // Delete and re-save the xml file in isolated storage
            string uniqueName = OnNameChanging(this.name, this.displayName);
            this.Name = uniqueName;

            // Save it
            DispatcherHelper.BeginInvoke(() =>
            {
                var saver = ApplicationViewModel.Instance.Container.Resolve<IViewModelSaver<ModelViewModel>>();
                saver.Save(this);
            });
        }

        private string OnNameChanging(string oldName, string newName)
        {
            // Get the source and new duplicate file names
            string sourceFileName = string.Format(@"{0}\{1}.xml", ApplicationSettings.DirectoryName, oldName);
            string uniqueName = CreateUniqueName(newName);
            string destinationFileName = string.Format(@"{0}\{1}.xml", ApplicationSettings.DirectoryName, uniqueName);

            // Copy the source file in isolated storage
            // TODO - this assumes it's offline/local
            using (var storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                storage.CopyFile(sourceFileName, destinationFileName);
                storage.DeleteFile(sourceFileName);
            }

            return uniqueName;
        }

        private string CreateUniqueName(string name)
        {
            var modelInfos = ApplicationViewModel.Instance.Library.Models;

            string newName = name;

            int i = 0;

            while (modelInfos.Any(mi => mi.Name == newName))
            {
                newName = string.Format("{0}({1})", name, ++i);
            }

            return newName;
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="CreatedDate"/> property.
        /// </summary>
        public DateTime CreatedDate
        {
            get
            {
                if (this.createdDate != DateTime.MinValue)
                {
                    return this.createdDate;
                }
                else
                {
                    return DateTime.Now;
                }
            }
            set
            {
                if (this.createdDate != value)
                {
                    this.createdDate = value;
                    OnPropertyChanged(() => CreatedDate);
                    OnPropertyChanged(() => CreatedDateString);
                }
            }
        }

        public string CreatedDateString
        {
            get { return this.CreatedDate.ToString(DateFormatString); }
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="ModifiedDate"/> property.
        /// </summary>
        public DateTime ModifiedDate
        {
            get
            {
                if (this.modifiedDate != DateTime.MinValue)
                {
                    return this.modifiedDate;
                }
                else
                {
                    return this.CreatedDate;
                }
            }
            set
            {
                if (this.modifiedDate != value)
                {
                    this.modifiedDate = value;
                    OnPropertyChanged(() => ModifiedDate);
                    OnPropertyChanged(() => ModifiedDateString);
                }
            }
        }

        public string ModifiedDateString
        {
            get { return this.ModifiedDate.ToString(DateFormatString); }
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="Description"/> property.
        /// </summary>
        public string Description
        {
            get { return this.description; }
            set
            {
                if (this.description != value)
                {
                    this.description = value;
                    OnPropertyChanged(() => Description);
                }
            }
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="Author"/> property.
        /// </summary>
        public string Author
        {
            get { return this.author; }
            set
            {
                if (this.author != value)
                {
                    this.author = value;
                    OnPropertyChanged(() => Author);
                }
            }
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="Versions"/> property.
        /// </summary>
        public int Versions
        {
            get { return this.versions; }
            set
            {
                if (this.versions != value)
                {
                    this.versions = value;
                    OnPropertyChanged(() => Versions);
                    OnPropertyChanged(() => VersionsDescription);
                }
            }
        }

        /// <summary>
        /// Gets the value of the <see cref="VersionsDescription"/> property.
        /// </summary>
        public string VersionsDescription
        {
            get { return string.Format("{0} versions", this.versions); }
        }

        /// <summary>
        /// Gets the value of the <see cref="Version"/> property.
        /// </summary>
        public string Version
        {
            get { return version; }
        }

        /// <summary>
        /// Gets or sets the zoom level which is auto mapped to the ZoomViewModel.ZoomLevel property for binding
        /// </summary>
        /// <value>
        /// The zoom level.
        /// </value>
        public double ZoomLevel
        {
            get { return this.mappedZoomLevel.GetValue<double>(); }
            set { this.mappedZoomLevel.SetValue(value); }
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="PanX"/> property.
        /// </summary>
        public double PanX
        {
            get { return this.panX; }
            set
            {
                if (this.panX != value)
                {
                    this.panX = value;
                    OnPropertyChanged(() => PanX);
                }
            }
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="PanY"/> property.
        /// </summary>
        public double PanY
        {
            get { return this.panY; }
            set
            {
                if (this.panY != value)
                {
                    this.panY = value;
                    OnPropertyChanged(() => PanY);
                }
            }
        }

        /// <summary>
        /// Resets this instance.
        /// </summary>
        public void Reset()
        {
            this.Rows = DefaultRows;
            this.Columns = DefaultColumns;

            this.relationshipViewModels.RemoveAll();

            foreach (var containerVM in this.containerViewModels)
            {
                this.variableViewModels.RemoveAll();
            }

            this.variableViewModels.RemoveAll();
            this.containerViewModels.RemoveAll();
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="Rows"/> property.
        /// </summary>
        /// <remarks>
        /// This sizes the grid to the specified number of rows.
        /// </remarks>
        public int Rows
        {
            get
            {
                if (rows != 0)
                {
                    return this.rows;
                }
                else
                {
                    return DefaultRows;
                }
            }
            set
            {
                if (rows != value)
                {
                    bool canChange = true;

                    // Check that there is not a container in this row
                    if (value < this.rows)
                    {
                        if (value < 1)
                        {
                            canChange = false;
                        }
                        else
                        {
                            int maxPossiblePositionY = value - 1;

                            // Get the list of position Y's, taking into account the different size's of containers.
                            var positionYs = this.containerViewModels.Select(cvm =>
                                                                                 {
                                                                                     if (cvm.SizeOne)
                                                                                         return cvm.PositionY;
                                                                                     if (cvm.SizeTwo)
                                                                                         return cvm.PositionY + 1;
                                                                                     if (cvm.SizeThree)
                                                                                         return cvm.PositionY + 2;
                                                                                     return 0;
                                                                                 }).ToList();
                            int maxActualPositionY = positionYs.Count > 0 ? positionYs.Max() : 0;

                            if (maxActualPositionY > maxPossiblePositionY)
                            {
                                // Don't allow the change
                                canChange = false;
                            }
                        }
                    }

                    if (canChange)
                        this.rows = value;

                    OnPropertyChanged(() => Rows);
                }
            }
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="Columns"/> property.
        /// </summary>
        /// <remarks>
        /// This sizes the grid to the specified number of columns.
        /// </remarks>
        public int Columns
        {
            get
            {
                if (this.columns != 0)
                {
                    return this.columns;
                }
                else
                {
                    return DefaultColumns;
                }
            }
            set
            {
                if (this.columns != value)
                {
                    bool canChange = true;

                    // Check that there is not a container in this column
                    if (value < this.columns)
                    {
                        if (value < 1)
                        {
                            canChange = false;
                        }
                        else
                        {
                            int maxPossiblePositionX = value - 1;

                            // Get the list of position X's, taking into account the different size's of containers.
                            var positionXs = this.containerViewModels.Select(cvm =>
                            {
                                if (cvm.SizeOne)
                                    return cvm.PositionX;
                                if (cvm.SizeTwo)
                                    return cvm.PositionX + 1;
                                if (cvm.SizeThree)
                                    return cvm.PositionX + 2;
                                return 0;
                            }).ToList();

                            int maxActualPositionX = positionXs.Count > 0 ? positionXs.Max() : 0;

                            if (maxActualPositionX > maxPossiblePositionX)
                            {
                                // Don't allow the change
                                canChange = false;
                            }
                        }
                    }

                    if (canChange)
                        this.columns = value;

                    OnPropertyChanged(() => Columns);
                }
            }
        }

        /// <summary>
        /// Get the VariableViewModel with the specified Id
        /// </summary>
        /// <param name="variableId"></param>
        /// <returns></returns>
        public VariableViewModel GetVariable(int variableId)
        {
            var variableVM = (from v in
                                  (from extVvm in variableViewModels select extVvm)
                                  .Concat(
                                      (from cvm in containerViewModels
                                       from intVvm in cvm.VariableViewModels
                                       select intVvm))
                              where v.Id == variableId
                              select v).FirstOrDefault();
            return variableVM;
        }

        #region Containers

        /// <summary>
        /// Gets the value of the <see cref="ContainerViewModels"/> property.
        /// </summary>
        public ContainerViewModels ContainerViewModels
        {
            get { return containerViewModels; }
        }

        public ContainerViewModel NewContainer(int positionX, int positionY)
        {
            var containerVM = new ContainerViewModel
                                  {
                                      PositionX = positionX,
                                      PositionY = positionY
                                  };

            containerVM.Id = IdFactory.NewContainerId(this);

            this.containerViewModels.Add(containerVM);
            return containerVM;
        }

        void containerViewModels_ItemRemoved(object sender, ItemEventArgs<ContainerViewModel> e)
        {
            var deletedContainerVM = e.Item;

            this.RelationshipViewModels.RemoveAll(rvm => rvm.ContainerViewModel == deletedContainerVM);
            this.RelationshipViewModels.RemoveAll(rvm => rvm.To.ContainerViewModel == deletedContainerVM);
        }

        #endregion

        #region Constant Variables

        /// <summary>
        /// Gets the value of the <see cref="VariableViewModels"/> property.
        /// </summary>
        public ViewModelCollection<VariableViewModel> VariableViewModels
        {
            get { return variableViewModels; }
        }

        public VariableViewModel NewConstant(int containerX, int containerY, double constantX, double constantY)
        {
            var variableVM = new VariableViewModel();

            variableVM.Id = IdFactory.NewVariableId(ApplicationViewModel.Instance.ActiveModel);
            variableVM.ContainerViewModel = null;
            variableVM.Type = VariableTypes.Constant;
            variableVM.CellX = containerX;
            variableVM.CellY = containerY;

            var intX = Convert.ToInt32(constantX);
            var intY = Convert.ToInt32(constantY);

            bool notEmpty = true;
            while (notEmpty)
            {
                // Check if there is already a variable at this position
                notEmpty = (from vvm in this.variableViewModels
                            where vvm.PositionX == intX && vvm.PositionY == intY
                            && vvm.CellX == containerX && vvm.CellX == containerY
                            select vvm).Any();
                if (notEmpty)
                {
                    intX += 5;
                    intY += 5;
                }
            }
            variableVM.PositionX = intX;
            variableVM.PositionY = intY;

            this.variableViewModels.Add(variableVM);
            return variableVM;
        }

        /// <summary>
        /// Moves the constant from one container to a different one
        /// </summary>
        /// <param name="constantVM">The constant VM.</param>
        /// <param name="containerX">The container X.</param>
        /// <param name="containerY">The container Y.</param>
        /// <param name="constantX">The constant X.</param>
        /// <param name="constantY">The constant Y.</param>
        /// <returns></returns>
        public void MoveConstant(VariableViewModel constantVM, int containerX, int containerY, double constantX, double constantY)
        {
            // Get the relationships it's involved in
            var relationshipVMs = this.RelationshipViewModels.Where(rvm => rvm.From == constantVM || rvm.To == constantVM)
                .ToList();

            // Remove the constant from the variables
            // This will remove it from its current ContainerSite
            this.VariableViewModels.Remove(constantVM);

            // Change its Cell and Position
            constantVM.CellX = containerX;
            constantVM.CellY = containerY;
            constantVM.PositionX = Convert.ToInt32(constantX);
            constantVM.PositionY = Convert.ToInt32(constantY);

            // Re-add it 
            this.VariableViewModels.Add(constantVM);

            // Re-draw the relationships
            relationshipVMs.ForEach(rvm => this.RelationshipViewModels.Add(rvm));
        }

        public void MoveContainer(ContainerViewModel containerVM, int containerX, int containerY)
        {
            // Get the relationships it's involved in
            var relationshipVMs = this.RelationshipViewModels.Where(rvm => rvm.From.ContainerViewModel == containerVM || rvm.To.ContainerViewModel == containerVM)
                .ToList();

            // This will remove it from its current ContainerSite
            this.ContainerViewModels.Remove(containerVM);

            // Change its Cell and Position
            containerVM.PositionX = containerX;
            containerVM.PositionY = containerY;

            // Update the variables
            foreach (var variableVM in containerVM.VariableViewModels)
            {
                variableVM.CellX = containerX;
                variableVM.CellY = containerY;
            }

            // Re-add it 
            this.ContainerViewModels.Add(containerVM);

            // Re-draw the relationships
            relationshipVMs.ForEach(rvm => this.RelationshipViewModels.Add(rvm));
            // TODO DispatcherHelper.QuadBeginInvoke(() => relationshipVMs.ForEach(rvm => this.RelationshipViewModels.Add(rvm)));
        }

        public void MoveVariable(VariableViewModel variableVM, ContainerViewModel containerVM, int positionX, int positionY)
        {
            // Get the relationships it's involved in
            var relationshipVMs = (from rvm in this.RelationshipViewModels
                                   where rvm.From == variableVM || rvm.To == variableVM
                                   select rvm)
                                  .ToList();

            // Remove the constant from the variables
            // This will remove it from its current ContainerSite
            var sourceContainer = variableVM.ContainerViewModel;
            sourceContainer.VariableViewModels.Remove(variableVM);

            // Change its Cell and Position
            variableVM.CellX = containerVM.PositionX;
            variableVM.CellY = containerVM.PositionY;
            variableVM.PositionX = positionX;
            variableVM.PositionY = positionY;

            // Re-add it 
            containerVM.VariableViewModels.Add(variableVM);

            variableVM.ContainerViewModel = containerVM;

            // Re-draw the relationships
            relationshipVMs.ForEach(rvm =>
                                        {
                                            rvm.ContainerViewModel = rvm.From.ContainerViewModel;
                                            this.RelationshipViewModels.Add(rvm);
                                        });
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

            // Delete any relationships this variable was involved in
            this.RelationshipViewModels.RemoveAll(rvm => rvm.From == variableVM || rvm.To == variableVM);
        }

        #endregion

        #region Relationships

        /// <summary>
        /// Gets the value of the <see cref="RelationshipViewModels"/> property.
        /// </summary>
        public ViewModelCollection<RelationshipViewModel> RelationshipViewModels
        {
            get { return relationshipViewModels; }
        }

        public RelationshipViewModel NewRelationship(VariableViewModel from, VariableViewModel to, RelationshipTypes type)
        {
            // It crosses containers
            // So add it to the Model ViewModel's relationships collection.

            // Add the relationship to the container it starts from
            var containerVM = from.ContainerViewModel;

            // Create the Relationship ViewModel
            var relationshipVM = new RelationshipViewModel();
            relationshipVM.Id = IdFactory.NewRelationshipId(this);
            relationshipVM.ContainerViewModel = containerVM;
            relationshipVM.Type = type;
            relationshipVM.From = from;
            relationshipVM.To = to;

            this.relationshipViewModels.Add(relationshipVM);

            return relationshipVM;
        }

        void relationshipViewModels_ItemRemoved(object sender, ItemEventArgs<RelationshipViewModel> e)
        {

        }

        #endregion

        public void Clear()
        {
            
        }

        public override string ToString()
        {
            return this.Name;
        }

        protected override void Dispose(bool disposing)
        {



            base.Dispose(disposing);
        }
    }
}
