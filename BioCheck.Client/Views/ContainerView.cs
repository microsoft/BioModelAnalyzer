using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using BioCheck.Controls;
using BioCheck.Services;
using BioCheck.ViewModel;
using BioCheck.ViewModel.Cells;
using BioCheck.Views.MembraneReceptors;
using MvvmFx.Common.Helpers;
using MvvmFx.Common.Helpers.WeakEventing;
using Microsoft.Practices.Unity;

namespace BioCheck.Views
{
    [TemplatePart(Name = TemplateParts.VariableCanvas, Type = typeof(Canvas))]
    [TemplatePart(Name = TemplateParts.ArrowCanvas, Type = typeof(Canvas))]
    [TemplatePart(Name = TemplateParts.OuterPath, Type = typeof(Path))]
    [TemplatePart(Name = TemplateParts.MembraneReceptorCanvas, Type = typeof(Canvas))]
    [TemplatePart(Name = TemplateParts.LayoutRoot, Type = typeof(Grid))]
    [TemplateVisualState(Name = VisualStates.NotDropTargetState, GroupName = VisualStates.DropTargetStatesGroup)]
    [TemplateVisualState(Name = VisualStates.IsDropTargetState, GroupName = VisualStates.DropTargetStatesGroup)]
    [TemplateVisualState(Name = VisualStates.MembraneDropTargetState, GroupName = VisualStates.DropTargetStatesGroup)]
    [TemplateVisualState(Name = VisualStates.CheckedState, GroupName = VisualStates.CheckedStatesGroup)]
    [TemplateVisualState(Name = VisualStates.UncheckedState, GroupName = VisualStates.CheckedStatesGroup)]
    public class ContainerView : Control,
                                 DragDropService<ToolbarCommand>.IDropTarget,
                                 DragDropService<VariableViewModel>.IDropTarget,
                                 DragDropService<ContainerViewModel>.IDragSource,
                                 CollectionSource<VariableViewModel>.ICollectionPresenter,
                                 IVariableHost,
                                 IDisposable
    {
        public struct TemplateParts
        {
            public const string VariableCanvas = "VariableCanvas";
            public const string ArrowCanvas = "ArrowCanvas";
            public const string OuterPath = "OuterPath";
            public const string MembraneReceptorCanvas = "MembraneReceptorCanvas";
            public const string LayoutRoot = "LayoutRoot";
        }

        private struct VisualStates
        {
            public const string DropTargetStatesGroup = "DropTargetStates";
            public const string NotDropTargetState = "NotDropTarget";
            public const string IsDropTargetState = "IsDropTarget";
            public const string MembraneDropTargetState = "MembraneDropTarget";

            public const string CheckedStatesGroup = "CheckStates";
            public const string CheckedState = "Checked";
            public const string UncheckedState = "Unchecked";
        }

        private IContextBarService contextBarService;
        private CollectionSource<VariableViewModel> variablesSource;

        public Canvas variableCanvas;
        public Canvas arrowCanvas;
        private Path outerPath;
        private Canvas membraneReceptorCanvas;

        private Grid layoutRoot;

        private bool isLoaded = false;

        public int PositionX { get; set; }
        public int PositionY { get; set; }

        private const int DefaultSnapSize = 25;

        public ContainerView()
        {
            this.DefaultStyleKey = typeof(ContainerView);

            this.Loaded += ContainerView_Loaded;

            this.variablesSource = new CollectionSource<VariableViewModel>(this);
        }

        void ContainerView_Loaded(object sender, RoutedEventArgs e)
        {
            this.isLoaded = true;

            this.ApplyTemplate();

            this.variablesSource.AddItems();

            // Reset the clipping region for the variable canvas
            ResetClipRegions();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.variableCanvas = this.GetTemplateChild(TemplateParts.VariableCanvas) as Canvas;
            if (this.variableCanvas == null)
                throw new MissingFieldException(TemplateParts.VariableCanvas);

            this.MouseLeftButtonDown += variableCanvas_MouseLeftButtonDown;
            this.MouseEnter += ContainerView_MouseEnter;
            this.arrowCanvas = this.GetTemplateChild(TemplateParts.ArrowCanvas) as Canvas;
            if (this.arrowCanvas == null)
                throw new MissingFieldException(TemplateParts.ArrowCanvas);

            this.outerPath = this.GetTemplateChild(TemplateParts.OuterPath) as Path;
            if (this.outerPath == null)
                throw new MissingFieldException(TemplateParts.OuterPath);
            this.outerPath.MouseLeftButtonDown += outerPath_MouseLeftButtonDown;

            this.membraneReceptorCanvas =
                this.GetTemplateChild(TemplateParts.MembraneReceptorCanvas) as Canvas;
            if (this.membraneReceptorCanvas == null)
                throw new MissingFieldException(TemplateParts.MembraneReceptorCanvas);

            this.layoutRoot = this.GetTemplateChild(TemplateParts.LayoutRoot) as Grid;
            if (this.layoutRoot == null)
                throw new MissingFieldException(TemplateParts.LayoutRoot);

            OnSizeChanged();
        }

        void ContainerView_MouseEnter(object sender, MouseEventArgs e)
        {
            var toolbarVM = ApplicationViewModel.Instance.ToolbarViewModel;
            if (toolbarVM.IsVariableActive)
            {
                this.Cursor = Cursors.Hand;
            }
            else
            {
                this.Cursor = Cursors.Arrow;
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
        }

        void variableCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
          
        }

        void outerPath_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
        }

        protected override void OnMouseRightButtonDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonDown(e);

            if (this.contextBarService == null)
            {
                this.contextBarService = ApplicationViewModel.Instance.Container.Resolve<IContextBarService>();
            }

            VisualStateManager.GoToState(this, VisualStates.CheckedState, true);
            this.contextBarService.Show(this.DataContext, e, "ContainerContextBarStyle");
            this.contextBarService.Closed += contextBarService_Closed;
        }

        void contextBarService_Closed(object sender, EventArgs e)
        {
            this.contextBarService.Closed -= contextBarService_Closed;
            VisualStateManager.GoToState(this, VisualStates.UncheckedState, true);
        }

        #region IsChecked

        /// <summary>
        /// Gets or sets the value of the <see cref="IsChecked"/> property.
        /// </summary>
        public bool IsChecked
        {
            get { return (bool)GetValue(IsCheckedProperty); }
            set { SetValue(IsCheckedProperty, value); }
        }

        /// <summary>
        /// The <see cref="IsCheckedProperty" /> dependency property registered with the 
        /// Microsoft Presentation Foundation (WPF) property system.
        /// </summary>
        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register("IsChecked", typeof(bool), typeof(ContainerView), new PropertyMetadata(false, OnIsCheckedChanged));

        private static void OnIsCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ContainerView)d).OnThisIsCheckedChanged(d, e);
        }

        private void OnThisIsCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (this.IsChecked)
            {
                VisualStateManager.GoToState(this, VisualStates.CheckedState, true);
            }
            else
            {
                VisualStateManager.GoToState(this, VisualStates.UncheckedState, true);
            }
        }

        #endregion

        #region Variable Template

        /// <summary>
        /// Gets or sets the value of the <see cref="VariableTemplate"/> property.
        /// </summary>
        public DataTemplate VariableTemplate
        {
            get { return (DataTemplate)GetValue(VariableTemplateProperty); }
            set { SetValue(VariableTemplateProperty, value); }
        }

        /// <summary>
        /// The <see cref="VariableTemplateProperty" /> dependency property registered with the 
        /// Silverlight property system.
        /// </summary>
        public static readonly DependencyProperty VariableTemplateProperty =
            DependencyProperty.Register("VariableTemplate", typeof(DataTemplate), typeof(ContainerView), new PropertyMetadata(null));

        #endregion

        #region VariablesSource

        public IEnumerable VariablesSource
        {
            get { return (IEnumerable)GetValue(VariablesSourceProperty); }
            set { SetValue(VariablesSourceProperty, value); }
        }

        public static readonly DependencyProperty VariablesSourceProperty =
            CollectionSource<VariableViewModel>.Register("VariablesSource", typeof(ContainerView));

        public void Reset(VariableViewModel item)
        {
            if (!this.isLoaded)
            {
                return;
            }

            if (this.variableCanvas != null)
                this.variableCanvas.Children.Clear();
        }

        public void AddItem(VariableViewModel variableVM)
        {
            if (!this.isLoaded)
            {
                return;
            }

            if (variableVM.Type == VariableTypes.MembraneReceptor)
            {
                MembraneReceptorSite site = GetMembraneReceptorSite(variableVM);

                if (site != null)
                {
                    var membraneView = site.AddMembraneReceptor(variableVM);
                }
            }
            else if (variableVM.Type == VariableTypes.Constant)
            {
                // Shouldn't get here
            }
            else
            {
                // load data template content
                var variable = this.VariableTemplate.LoadContent() as VariableView;

                if (variable != null)
                {
                    // set data context
                    variable.DataContext = variableVM;

                    this.variableCanvas.Children.Add(variable);
                }
            }
        }

        private MembraneReceptorSite GetMembraneReceptorSite(VariableViewModel variableVM)
        {
            var varPoint = new Point(variableVM.PositionX, variableVM.PositionY);
            int varAngle = variableVM.Angle.GetValueOrDefault();

            MembraneReceptorSite site = null;

            foreach (var child in this.membraneReceptorCanvas.Children)
            {
                var receptorSite = (MembraneReceptorSite)child;

                if (variableVM.Angle.HasValue)
                {
                    int siteAngle = receptorSite.Angle;

                    if (siteAngle == varAngle)
                    {
                        double siteX = Canvas.GetLeft(receptorSite);
                        double siteY = Canvas.GetTop(receptorSite);

                        // Update the variable position to the site position
                        variableVM.PositionX = Convert.ToInt32(siteX - 7);
                        variableVM.PositionY = Convert.ToInt32(siteY - 10);
                        site = receptorSite;
                        break;
                    }
                }
                else
                {
                    double siteX = Canvas.GetLeft(receptorSite);
                    double siteY = Canvas.GetTop(receptorSite);

                    Rect siteRect = new Rect(Convert.ToInt32(siteX) - 10, Convert.ToInt32(siteY) - 10, 50, 50);

                    if (siteRect.Contains(varPoint))
                    {
                        site = receptorSite;

                        // VERSION 3
                        // Set the variable Angle to the Site Angle
                        variableVM.Angle = site.Angle;

                        break;
                    }
                }
            }
            return site;
        }

        public void RemoveItem(VariableViewModel variableVM)
        {
            if (variableVM.Type == VariableTypes.MembraneReceptor)
            {
                MembraneReceptorSite site = GetMembraneReceptorSite(variableVM);

                if (site != null)
                {
                    site.RemoveMembraneReceptor();
                }
            }
            else
            {
                VariableView variableToRemove = this.variableCanvas.Children.OfType<VariableView>().First(variableView => variableView.DataContext == variableVM);

                this.variableCanvas.Children.Remove(variableToRemove);
                variableToRemove.Dispose();
            }
        }

        #endregion

        #region Drag/Drop

        #region ToolbarCommand

        public void DragDropEnter(ToolbarCommand dataContext, MouseEventArgs mouseEvent)
        {
            if (dataContext.CommandType == CommandType.Variable)
            {
                VisualStateManager.GoToState(this, VisualStates.IsDropTargetState, true);
            }
        }

        public void DragDropExit(ToolbarCommand dataContext)
        {
            if (dataContext.CommandType == CommandType.Variable ||
                dataContext.CommandType == CommandType.MembraneReceptor)
            {
                VisualStateManager.GoToState(this, VisualStates.NotDropTargetState, true);
            }
        }

        public int SnapSize
        {
            get { return DefaultSnapSize; }
        }

        public void ThumbnailDroping(ToolbarCommand dataContext, FrameworkElement cursor, Point cursorPosition)
        {
            if (dataContext.CommandType == CommandType.Variable)
            {
                DragDropExit(dataContext);

                ApplicationViewModel.Instance.DupActiveModel();

                var containerVM = (ContainerViewModel)this.DataContext;

                var positionX = Convert.ToInt32(cursorPosition.X) - VariableView.DefaultFixedWidth / 2;
                var positionY = Convert.ToInt32(cursorPosition.Y) - VariableView.DefaultFixedHeight / 2;

                var variableVM = containerVM.NewVariable(positionX, positionY);
            }
        }

        #endregion

        #region Drag Source

        private FrameworkElement dragCursor;

        public FrameworkElement DragCursor
        {
            get
            {
                if (dragCursor == null)
                {
                    var containerView = new ContainerView();

                    //  var containerVM = (ContainerViewModel) this.DataContext;

                    containerView.Style = App.Current.Resources["ContainerViewDragStyle"] as Style;

                    containerView.Opacity = 0.6;
                    containerView.Width = ApplicationSettings.CellWidth / 2;
                    containerView.Height = ApplicationSettings.CellHeight / 2;
                    //containerView.Width = containerVM.Width / 2;
                    //containerView.Height = containerVM.Height / 2;
                    //containerView.DataContext = this.DataContext;

                    dragCursor = containerView;
                }

                this.Opacity = 0.4;

                return dragCursor;
            }
        }

        public ContainerViewModel Payload
        {
            get { return this.DataContext as ContainerViewModel; }
        }

        public void Dropped()
        {
            this.Opacity = 1;

            this.IsChecked = false;
        }

        #endregion

        #region Existing Variable

        public void DragDropEnter(VariableViewModel dataContext, MouseEventArgs mouseEvent)
        {
            if (dataContext.Type == VariableTypes.Default)
            {
                VisualStateManager.GoToState(this, VisualStates.IsDropTargetState, true);
            }
        }

        public void DragDropExit(VariableViewModel dataContext)
        {
            VisualStateManager.GoToState(this, VisualStates.NotDropTargetState, true);
        }

        public void ThumbnailDroping(VariableViewModel variableVM, FrameworkElement cursor, Point cursorPosition)
        {
            if (variableVM.Type != VariableTypes.Default)
                return;

            DragDropExit(variableVM);

            var containerVM = (ContainerViewModel)this.DataContext;

            var positionX = Convert.ToInt32(cursorPosition.X) - VariableView.DefaultFixedWidth / 2;
            var positionY = Convert.ToInt32(cursorPosition.Y) - VariableView.DefaultFixedHeight / 2;

            ApplicationViewModel.Instance.ActiveModel.MoveVariable(variableVM, containerVM, positionX, positionY);

            var toolbarVM = ApplicationViewModel.Instance.ToolbarViewModel;
            toolbarVM.MouseDownIsHandled = false;
        }

        #endregion

        #endregion

        #region Size

        /// <summary>
        /// Gets or sets the value of the <see cref="Size"/> property.
        /// </summary>
        public ContainerSizeTypes Size
        {
            get { return (ContainerSizeTypes)GetValue(SizeProperty); }
            set { SetValue(SizeProperty, value); }
        }

        /// <summary>
        /// The <see cref="SizeProperty" /> dependency property registered with the 
        /// Microsoft Presentation Foundation (WPF) property system.
        /// </summary>
        public static readonly DependencyProperty SizeProperty =
            DependencyProperty.Register("Size", typeof(ContainerSizeTypes), typeof(ContainerView),
            new PropertyMetadata(ContainerSizeTypes.One, OnSizeChanged));

        private static void OnSizeChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            ((ContainerView)dependencyObject).OnSizeChanged(dependencyPropertyChangedEventArgs);
        }

        private ContainerGrid containerGrid;

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

        private void OnSizeChanged(DependencyPropertyChangedEventArgs args)
        {
            if (!this.isLoaded)
            {
                return;
            }
            if (this.DataContext == null)
            {
                return;
            }

            // Update the DataContext's of the surrounding ContainerSites when the size changes.
            var oldValue = args.OldValue;
            var newValue = args.NewValue;
            if (oldValue != null)
            {
                var containerVM = (ContainerViewModel)this.DataContext;

                // Clear the DataContext for the old sites
                var oldSize = EnumHelper.Parse<ContainerSizeTypes>(oldValue.ToString());
                var containerSites = Grid.GetContainerSites(oldSize, containerVM.PositionX, containerVM.PositionY);
                containerSites.ForEach(site => site.DataContext = null);

                if (newValue != null)
                {
                    // Set the DataContext for the new sites
                    var newSize = EnumHelper.Parse<ContainerSizeTypes>(newValue.ToString());
                    containerSites = Grid.GetContainerSites(newSize, containerVM.PositionX, containerVM.PositionY);
                    containerSites.ForEach(site => site.DataContext = containerVM);
                }
            }

            OnSizeChanged();
        }

        /// <summary>
        /// Called when [size changed].
        /// Sets the Clipping region for the Variable Canvas if the size changes, so that
        /// variables can only be painted in the right areas.
        /// </summary>
        public void OnSizeChanged()
        {
            if (!this.isLoaded)
            {
                return;
            }
            if (this.DataContext == null)
            {
                return;
            }
            
            // Reset the clipping region for the variable canvas
            ResetClipRegions();

            // Update the Membrane Receptor Sites
            var siteInfos = MembraneReceptorFactory.Create(this.Size);

            foreach (var child in this.membraneReceptorCanvas.Children)
            {
                var membraneReceptorSite = (MembraneReceptorSite)child;

                var siteInfo = siteInfos[0];

                Canvas.SetLeft(membraneReceptorSite, siteInfo.Left);
                Canvas.SetTop(membraneReceptorSite, siteInfo.Top);


                var membraneView = membraneReceptorSite.GetMembraneReceptor();
                if(membraneView != null)
                {
                    // Update the PositionX and Y of the VariableViewModel
                    var variableVM = (VariableViewModel) membraneView.DataContext;
                    variableVM.PositionX = Convert.ToInt32(siteInfo.Left - 7);
                    variableVM.PositionY = Convert.ToInt32(siteInfo.Top - 10);
                }

                siteInfos.RemoveAt(0);
            }

            // Reset the membrane receptors, this will re-draw their relationships to the new positions
            DispatcherHelper.DoubleBeginInvoke(() => this.Payload.ResetMembraneReceptors());
        }

        private void ResetClipRegions()
        {
            var containerVM = this.DataContext as ContainerViewModel;
            if (containerVM == null)
                return;

            var width = containerVM.Width / 2.0;
            var height = containerVM.Height / 2.0;

            width -= 20;
            height -= 20;

            var ellipseGeometry = new EllipseGeometry();
            ellipseGeometry.Center = new Point(width, height);
            ellipseGeometry.RadiusX = width;
            ellipseGeometry.RadiusY = height;

            this.variableCanvas.Clip = ellipseGeometry;
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

        ~ContainerView()
        {
            Dispose(false);
        }

        protected virtual void OnDispose()
        {
            this.DataContext = null;
            this.Template = null;

            if (this.variableCanvas != null)
                this.variableCanvas.Children.Clear();

            this.variablesSource.Dispose();
            this.variablesSource = null;
        }

        #endregion
    }
}
