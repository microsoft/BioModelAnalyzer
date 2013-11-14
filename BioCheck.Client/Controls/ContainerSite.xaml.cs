using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BioCheck.Services;
using BioCheck.ViewModel;
using BioCheck.ViewModel.Cells;
using BioCheck.ViewModel.Editing;
using BioCheck.Views;
using Microsoft.Practices.Unity;
using MvvmFx.Common.Helpers;

namespace BioCheck.Controls
{
    public partial class ContainerSite : UserControl,
                         DragDropService<ToolbarCommand>.IDropTarget,
                         DragDropService<VariableViewModel>.IDropTarget,
                         DragDropService<ContainerViewModel>.IDropTarget,
                         IVariableHost
    {
        private struct VisualStates
        {
            public const string DropTargetStatesGroup = "DropTargetStates";
            public const string NotDropTargetState = "NotDropTarget";
            public const string IsDropTargetState = "IsDropTarget";
            public const string InvalidDropTargetState = "InvalidDropTarget";

            public const string StableStates = "StableStates";
            public const string ClearStability = "ClearStability";
            public const string IsStableState = "IsStableState";
            public const string NotStableState = "NotStableState";
        }

        private const int DefaultSnapSize = 25;

        private IContextBarService contextBarService;
        private ContainerGrid containerGrid;
        List<ContainerSite> groupedContainers = new List<ContainerSite>();

        public ContainerSite()
        {
            // Required to initialize variables
            InitializeComponent();
        }

        private ContainerGrid Grid
        {
            get
            {
                // Get the parent ContainerGrid
                if (this.containerGrid == null)
                    this.containerGrid = SilverlightVisualTreeHelper.TryFindParent<ContainerGrid>(this);

                return this.containerGrid;
            }
        }

        #region Drag/Drop

        #region ToolbarCommand

        public void DragDropEnter(ToolbarCommand toolbarCommand, MouseEventArgs mouseEvent)
        {
            if (toolbarCommand.CommandType == CommandType.Container)
            {
                // If it's a container, check there's no constants here
                if (this.IsEmpty())
                {
                    VisualStateManager.GoToState(this, VisualStates.IsDropTargetState, true);
                }
                else
                {
                    VisualStateManager.GoToState(this, VisualStates.InvalidDropTargetState, true);
                }
            }
            else if (toolbarCommand.CommandType == CommandType.Constant)
            {
                // If it's a constant, check there's no container here
                if (!this.HasContainerOverlapping())
                {
                    VisualStateManager.GoToState(this, VisualStates.IsDropTargetState, true);
                }
                else
                {
                    VisualStateManager.GoToState(this, VisualStates.InvalidDropTargetState, true);
                }
            }
        }

        public void DragDropExit(ToolbarCommand toolbarCommand)
        {
            VisualStateManager.GoToState(this, VisualStates.NotDropTargetState, true);
        }

        public int SnapSize
        {
            get { return DefaultSnapSize; }
        }

        public void ThumbnailDroping(ToolbarCommand toolbarCommand, FrameworkElement cursor, Point cursorPosition)
        {
            DragDropExit(toolbarCommand);

            if (toolbarCommand.CommandType == CommandType.Container)
            {
                // If it's a container, check there's no constants here
                if (this.IsEmpty())
                {
                    ApplicationViewModel.Instance.DupActiveModel();

                    var containerVM =
                        ApplicationViewModel.Instance.ActiveModel.NewContainer(this.PositionX, this.PositionY);
                }
            }
            else if (toolbarCommand.CommandType == CommandType.Constant)
            {
                // If it's a constant, check there's no container here
                if (!this.HasContainerOverlapping())
                {
                    ApplicationViewModel.Instance.DupActiveModel();

                    var containerVM =
                        ApplicationViewModel.Instance.ActiveModel.NewConstant(this.PositionX, this.PositionY,
                                                                              cursorPosition.X, cursorPosition.Y);
                }
            }

            ApplicationViewModel.Instance.SaveActiveModel();
        }

        #endregion

        #region Existing Variable

        public void DragDropEnter(VariableViewModel dataContext, MouseEventArgs mouseEvent)
        {
            if (dataContext.Type == VariableTypes.Constant)
            {
                // If it's a constant, check there's no container here
                if (!this.HasContainerOverlapping())
                {
                    VisualStateManager.GoToState(this, VisualStates.IsDropTargetState, true);
                }
                else
                {
                    VisualStateManager.GoToState(this, VisualStates.InvalidDropTargetState, true);
                }
            }
        }

        public void DragDropExit(VariableViewModel dataContext)
        {
            VisualStateManager.GoToState(this, VisualStates.NotDropTargetState, true);
        }

        public void ThumbnailDroping(VariableViewModel constantVM, FrameworkElement cursor, Point cursorPosition)
        {
            DragDropExit(constantVM);

            if (constantVM.Type == VariableTypes.Constant)
            {
                // If it's a constant, check there's no container here
                if (!this.HasContainerOverlapping())
                {
                    // It's coming from a different container site, move it
                    ApplicationViewModel.Instance.ActiveModel.MoveConstant(constantVM,
                                                                          this.PositionX, this.PositionY,
                                                                          cursorPosition.X, cursorPosition.Y);

                    var toolbarVM = ApplicationViewModel.Instance.ToolbarViewModel;
                    toolbarVM.MouseDownIsHandled = false;
                }
            }
        }

        #endregion

        #region Existing Container

        public void DragDropEnter(ContainerViewModel containerVM, MouseEventArgs mouseEvent)
        {
            // Check there's no container or constant here 
            if (containerVM.SizeOne)
            {
                // Check it's empty and not the same cell
                if (IsEmpty())
                {
                    VisualStateManager.GoToState(this, VisualStates.IsDropTargetState, true);
                }
                else if (!IsThisCell(containerVM))
                {
                    VisualStateManager.GoToState(this, VisualStates.InvalidDropTargetState, true);
                }
            }
            else if (containerVM.SizeTwo)
            {
                // It's a 2x2 size container. Need to check the 3 cells around it
                // If this cell is to be the top left, we need to set the drop states of the other 3 neighbouring sites
                SetDropStates(containerVM, new List<ContainerSite>
                                             {
                                                Grid.GetContainerSite(this.PositionX + 1, this.PositionY),
                                                Grid.GetContainerSite(this.PositionX, this.PositionY + 1),
                                                Grid.GetContainerSite(this.PositionX + 1, this.PositionY + 1),
                                             });
            }
            else if (containerVM.SizeThree)
            {
                // It's a 3x3 size container. Need to treat this one as the middle cell and check the 8 cells around it
                // If this cell is to be the centre, we need to set the drop states of the other 8 neighbouring sites
                SetDropStates(containerVM, new List<ContainerSite>
                                             {
                                                Grid.GetContainerSite(this.PositionX -1, this.PositionY - 1),
                                                Grid.GetContainerSite(this.PositionX, this.PositionY - 1),
                                                Grid.GetContainerSite(this.PositionX + 1, this.PositionY - 1),
                                                Grid.GetContainerSite(this.PositionX - 1, this.PositionY),
                                                Grid.GetContainerSite(this.PositionX + 1, this.PositionY),
                                                Grid.GetContainerSite(this.PositionX - 1, this.PositionY + 1),
                                                Grid.GetContainerSite(this.PositionX, this.PositionY + 1),
                                                Grid.GetContainerSite(this.PositionX + 1, this.PositionY + 1),
                                             });
            }
        }

        private void SetDropStates(ContainerViewModel containerVM, List<ContainerSite> neighbourCells)
        {
            string stateName = CanDrop(containerVM, neighbourCells) ? VisualStates.IsDropTargetState : VisualStates.InvalidDropTargetState;

            VisualStateManager.GoToState(this, stateName, true);
            groupedContainers.ForEach(neighbour => VisualStateManager.GoToState(neighbour, stateName, true));
        }

        private bool CanDrop(ContainerViewModel containerVM, List<ContainerSite> neighbourCells)
        {
            bool canDrop = true;

            neighbourCells.ForEach(neighbour =>
            {
                if (neighbour == null)
                {
                    canDrop = false;
                }
                else
                {
                    groupedContainers.Add(neighbour);

                    if (!neighbour.IsEmpty() && !neighbour.IsThisCell(containerVM))
                    {
                        canDrop = false;
                    }
                }
            });

            if (!this.IsEmpty() && !this.IsThisCell(containerVM))
            {
                canDrop = false;
            }

            return canDrop;
        }

        public void DragDropExit(ContainerViewModel dataContext)
        {
            VisualStateManager.GoToState(this, VisualStates.NotDropTargetState, true);

            this.groupedContainers.ForEach(site => VisualStateManager.GoToState(site, VisualStates.NotDropTargetState, true));
            this.groupedContainers.Clear();
        }

        public void ThumbnailDroping(ContainerViewModel containerVM, FrameworkElement cursor, Point cursorPosition)
        {
            DragDropExit(containerVM);

            bool canDrop = false;

            if (containerVM.SizeOne)
            {
                canDrop = IsEmpty();
            }
            else if (containerVM.SizeTwo)
            {
                // It's a 2x2 size container. Need to check the 3 cells around it
                // If this cell is to be the top left, we need to set the drop states of the other 3 neighbouring sites
                canDrop = CanDrop(containerVM, new List<ContainerSite>
                                               {
                                                   Grid.GetContainerSite(this.PositionX + 1, this.PositionY),
                                                   Grid.GetContainerSite(this.PositionX, this.PositionY + 1),
                                                   Grid.GetContainerSite(this.PositionX + 1, this.PositionY + 1),
                                               });
            }
            else if (containerVM.SizeThree)
            {
                // It's a 3x3 size container. Need to treat this one as the middle cell and check the 8 cells around it
                // If this cell is to be the centre, we need to set the drop states of the other 8 neighbouring sites
                canDrop = CanDrop(containerVM, new List<ContainerSite>
                                                   {
                                                       Grid.GetContainerSite(this.PositionX - 1, this.PositionY - 1),
                                                       Grid.GetContainerSite(this.PositionX, this.PositionY - 1),
                                                       Grid.GetContainerSite(this.PositionX + 1, this.PositionY - 1),
                                                       Grid.GetContainerSite(this.PositionX - 1, this.PositionY),
                                                       Grid.GetContainerSite(this.PositionX + 1, this.PositionY),
                                                       Grid.GetContainerSite(this.PositionX - 1, this.PositionY + 1),
                                                       Grid.GetContainerSite(this.PositionX, this.PositionY + 1),
                                                       Grid.GetContainerSite(this.PositionX + 1, this.PositionY + 1),
                                                   });
            }

            // Check there's no container or constant here 
            if (canDrop)
            {
                if (containerVM.SizeThree)
                {
                    // If it's a size 3 container, then set this one as the centre, not the top left
                    ApplicationViewModel.Instance.ActiveModel.MoveContainer(containerVM,
                                                                            this.PositionX - 1, this.PositionY - 1);
                }
                else
                {
                    // It's coming from a different container site, move it
                    ApplicationViewModel.Instance.ActiveModel.MoveContainer(containerVM,
                                                                            this.PositionX, this.PositionY);
                }

                var toolbarVM = ApplicationViewModel.Instance.ToolbarViewModel;
                toolbarVM.MouseDownIsHandled = false;
            }
        }

        public bool HasContainer()
        {
            var modelVM = ApplicationViewModel.Instance.ActiveModel;
            bool hasContainer = modelVM.ContainerViewModels.HasContainer(this.PositionX, this.PositionY);
            return hasContainer;
        }

        public bool HasContainerOverlapping()
        {
            var modelVM = ApplicationViewModel.Instance.ActiveModel;
            bool hasContainer = modelVM.ContainerViewModels.HasContainerOverlapping(this.PositionX, this.PositionY);
            return hasContainer;
        }

        public bool HasConstants()
        {
            bool hasConstants = this.VariableCanvas.Children.Count > 0;
            return hasConstants;
        }

        public bool IsEmpty()
        {
            return !HasContainerOverlapping() && !HasConstants();
        }

        public bool IsThisCell(ContainerViewModel dataContext)
        {
            bool isThisCell = dataContext == this.DataContext;
            return isThisCell;
        }

        #endregion

        #endregion

        public VariableView AddConstantVariable(VariableViewModel variableVM)
        {
            var variableView = new VariableView();

            var constantStyle = App.Current.Resources["ConstantVariableViewStyle"];
            variableView.Style = constantStyle as Style;
            variableView.DataContext = variableVM;

            this.VariableCanvas.Children.Add(variableView);

            return variableView;
        }

        public VariableView GetConstantVariableView(VariableViewModel variableVM)
        {
            var variableView = this.VariableCanvas.Children
                .OfType<VariableView>()
                .FirstOrDefault(varV => varV.DataContext == variableVM);

            return variableView;
        }

        public void RemoveConstantVariable(VariableViewModel variableVM)
        {
            var variableView = GetConstantVariableView(variableVM);
            if (variableView != null)
            {
                variableView.Dispose();
                VariableCanvas.Children.Remove(variableView);
            }
        }

        public void Reset()
        {
            this.DataContext = null;

            this.VariableCanvas.Children.Clear();
        }

        #region Show Borders

        /// <summary>
        /// Gets or sets the value of the <see cref="ShowLeft"/> property.
        /// </summary>
        public bool ShowLeft
        {
            get { return (bool)GetValue(ShowLeftProperty); }
            set { SetValue(ShowLeftProperty, value); }
        }

        /// <summary>
        /// The <see cref="ShowLeftProperty" /> dependency property registered with the 
        /// Microsoft Presentation Foundation (WPF) property system.
        /// </summary>
        public static readonly DependencyProperty ShowLeftProperty =
            DependencyProperty.Register("ShowLeft", typeof(bool), typeof(ContainerSite), new PropertyMetadata(true, OnShowLeftChanged));

        private static void OnShowLeftChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ContainerSite)d).OnThisShowLeftChanged(d, e);
        }

        private void OnThisShowLeftChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            this.LeftPath.Visibility = this.ShowLeft
                                                ? Visibility.Visible
                                                : Visibility.Collapsed;
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="ShowTop"/> property.
        /// </summary>
        public bool ShowTop
        {
            get { return (bool)GetValue(ShowTopProperty); }
            set { SetValue(ShowTopProperty, value); }
        }

        /// <summary>
        /// The <see cref="ShowTopProperty" /> dependency property registered with the 
        /// Microsoft Presentation Foundation (WPF) property system.
        /// </summary>
        public static readonly DependencyProperty ShowTopProperty =
            DependencyProperty.Register("ShowTop", typeof(bool), typeof(ContainerSite), new PropertyMetadata(true, OnShowTopChanged));

        private static void OnShowTopChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ContainerSite)d).OnThisShowTopChanged(d, e);
        }

        private void OnThisShowTopChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            this.TopPath.Visibility = this.ShowTop
                                             ? Visibility.Visible
                                             : Visibility.Collapsed;
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="ShowRight"/> property.
        /// </summary>
        public bool ShowRight
        {
            get { return (bool)GetValue(ShowRightProperty); }
            set { SetValue(ShowRightProperty, value); }
        }

        /// <summary>
        /// The <see cref="ShowRightProperty" /> dependency property registered with the 
        /// Microsoft Presentation Foundation (WPF) property system.
        /// </summary>
        public static readonly DependencyProperty ShowRightProperty =
            DependencyProperty.Register("ShowRight", typeof(bool), typeof(ContainerSite), new PropertyMetadata(true, OnShowRightChanged));

        private static void OnShowRightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ContainerSite)d).OnThisShowRightChanged(d, e);
        }

        private void OnThisShowRightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            this.RightPath.Visibility = this.ShowRight
                                             ? Visibility.Visible
                                             : Visibility.Collapsed;
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="ShowBottom"/> property.
        /// </summary>
        public bool ShowBottom
        {
            get { return (bool)GetValue(ShowBottomProperty); }
            set { SetValue(ShowBottomProperty, value); }
        }

        /// <summary>
        /// The <see cref="ShowBottomProperty" /> dependency property registered with the 
        /// Microsoft Presentation Foundation (WPF) property system.
        /// </summary>
        public static readonly DependencyProperty ShowBottomProperty =
            DependencyProperty.Register("ShowBottom", typeof(bool), typeof(ContainerSite), new PropertyMetadata(true, OnShowBottomChanged));

        private static void OnShowBottomChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ContainerSite)d).OnThisShowBottomChanged(d, e);
        }

        private void OnThisShowBottomChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            this.BottomPath.Visibility = this.ShowBottom
                                             ? Visibility.Visible
                                             : Visibility.Collapsed;
        }

        #endregion

        #region Stability

        /// <summary>
        /// Gets or sets the value of the <see cref="ShowStability"/> property.
        /// </summary>
        public bool ShowStability
        {
            get { return (bool)GetValue(ShowStabilityProperty); }
            set { SetValue(ShowStabilityProperty, value); }
        }

        /// <summary>
        /// The <see cref="ShowStabilityProperty" /> dependency property registered with the 
        /// Microsoft Presentation Foundation (WPF) property system.
        /// </summary>
        public static readonly DependencyProperty ShowStabilityProperty =
            DependencyProperty.Register("ShowStability", typeof(bool), typeof(ContainerSite), new PropertyMetadata(false, OnShowStabilityChanged));

        private static void OnShowStabilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ContainerSite)d).OnThisShowStabilityChanged(d, e);
        }

        private void OnThisShowStabilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SetStability();
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="IsStable"/> property.
        /// </summary>
        public bool IsStable
        {
            get { return (bool)GetValue(IsStableProperty); }
            set { SetValue(IsStableProperty, value); }
        }

        /// <summary>
        /// The <see cref="IsStableProperty" /> dependency property registered with the 
        /// Microsoft Presentation Foundation (WPF) property system.
        /// </summary>
        public static readonly DependencyProperty IsStableProperty =
            DependencyProperty.Register("IsStable", typeof(bool), typeof(ContainerSite), new PropertyMetadata(true, OnIsStableChanged));

        private static void OnIsStableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ContainerSite)d).OnThisIsStableChanged(d, e);
        }

        private void OnThisIsStableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SetStability();
        }

        private void SetStability()
        {
            // TODO - check this isn't needed and the model.ShowStability isn't needed
            //if (this.ShowStability && this.HasContainerOverlapping())
            if (this.ShowStability)
            {
                if (this.IsStable)
                {
                    VisualStateManager.GoToState(this, VisualStates.IsStableState, true);
                }
                else
                {
                    VisualStateManager.GoToState(this, VisualStates.NotStableState, true);
                }
            }
            else
            {
                VisualStateManager.GoToState(this, VisualStates.ClearStability, true);
            }
        }

        #endregion

        #region Position

        public int PositionX { get; set; }

        public int PositionY { get; set; }

        #endregion

        #region Context Bar

        protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
        {
            //TODO VisualStateManager.GoToState(this, VisualStates.NotDropTargetState, true);
        }

        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonDown(e);

            if (this.contextBarService == null)
                this.contextBarService = ApplicationViewModel.Instance.Container.Resolve<IContextBarService>();

            this.contextBarService.Close();

            // Only show a context menu if we can paste at the moment
            var contextBarVM = new ContainerSiteContextBarViewModel(this);

            var containerVM = CopyPasteManager.Clipboard as ContainerViewModel;
            if (containerVM != null)
            {
                if (containerVM.SizeOne)
                {
                    bool canPaste = this.IsEmpty();
                    if (canPaste)
                    {
                        VisualStateManager.GoToState(this, VisualStates.IsDropTargetState, true);
                    }
                    else
                    {
                        VisualStateManager.GoToState(this, VisualStates.InvalidDropTargetState, true);
                    }
                }
                else if (containerVM.SizeTwo)
                {
                    // It's a 2x2 size container. Need to check the 3 cells around it
                    // If this cell is to be the top left, we need to set the drop states of the other 3 neighbouring sites
                    SetPasteStates(new List<ContainerSite>
                                               {
                                                   Grid.GetContainerSite(this.PositionX + 1, this.PositionY),
                                                   Grid.GetContainerSite(this.PositionX, this.PositionY + 1),
                                                   Grid.GetContainerSite(this.PositionX + 1, this.PositionY + 1),
                                               });
                }
                else if (containerVM.SizeThree)
                {
                    // It's a 3x3 size container. Need to treat this one as the middle cell and check the 8 cells around it
                    // If this cell is to be the centre, we need to set the drop states of the other 8 neighbouring sites
                    SetPasteStates(new List<ContainerSite>
                                                   {
                                                       Grid.GetContainerSite(this.PositionX - 1, this.PositionY - 1),
                                                       Grid.GetContainerSite(this.PositionX, this.PositionY - 1),
                                                       Grid.GetContainerSite(this.PositionX + 1, this.PositionY - 1),
                                                       Grid.GetContainerSite(this.PositionX - 1, this.PositionY),
                                                       Grid.GetContainerSite(this.PositionX + 1, this.PositionY),
                                                       Grid.GetContainerSite(this.PositionX - 1, this.PositionY + 1),
                                                       Grid.GetContainerSite(this.PositionX, this.PositionY + 1),
                                                       Grid.GetContainerSite(this.PositionX + 1, this.PositionY + 1),
                                                   });
                }
            }
            else
            {
                var variableVM = CopyPasteManager.Clipboard as VariableViewModel;
                if (variableVM != null)
                {
                    bool canPaste = (variableVM.Type == VariableTypes.Constant && !this.HasContainerOverlapping());
                    if (canPaste)
                    {
                        VisualStateManager.GoToState(this, VisualStates.IsDropTargetState, true);
                    }
                    else
                    {
                        VisualStateManager.GoToState(this, VisualStates.InvalidDropTargetState, true);
                    }
                }
            }

            this.contextBarService.Show(contextBarVM, e);
            this.contextBarService.Closed += contextBarService_Closed;
        }

        private void SetPasteStates(List<ContainerSite> neighbourCells)
        {
            string stateName = CanPaste(neighbourCells) ? VisualStates.IsDropTargetState : VisualStates.InvalidDropTargetState;

            VisualStateManager.GoToState(this, stateName, true);
            groupedContainers.ForEach(neighbour => VisualStateManager.GoToState(neighbour, stateName, true));
        }

        private bool CanPaste(List<ContainerSite> neighbourCells)
        {
            bool canPaste = true;

            neighbourCells.ForEach(neighbour =>
            {
                if (neighbour == null)
                {
                    canPaste = false;
                }
                else
                {
                    groupedContainers.Add(neighbour);

                    if (!neighbour.IsEmpty())
                    {
                        canPaste = false;
                    }
                }
            });

            if (!this.IsEmpty())
            {
                canPaste = false;
            }

            return canPaste;
        }

        void contextBarService_Closed(object sender, EventArgs e)
        {
            this.contextBarService.Closed -= contextBarService_Closed;

            VisualStateManager.GoToState(this, VisualStates.NotDropTargetState, true);

            this.groupedContainers.ForEach(site => VisualStateManager.GoToState(site, VisualStates.NotDropTargetState, true));
            this.groupedContainers.Clear();
        }

        #endregion
    }
}