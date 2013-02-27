using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using BioCheck.Services;
using BioCheck.ViewModel;
using BioCheck.ViewModel.Cells;
using BioCheck.Views;
using MvvmFx.Common.Helpers;

namespace BioCheck.Controls
{
    public partial class MembraneReceptorSite : UserControl,
                                                DragDropService<ToolbarCommand>.IDropTarget
    {
        private struct VisualStates
        {
            public const string DropTargetStatesGroup = "DropTargetStates";
            public const string NotDropTargetState = "NotDropTarget";
            public const string IsDropTargetState = "IsDropTarget";
        }

        public MembraneReceptorSite()
        {
            // Required to initialize variables
            InitializeComponent();
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

        }

        public void DragDropEnter(ToolbarCommand dataContext, MouseEventArgs mouseEvent)
        {
            if (dataContext.CommandType == CommandType.MembraneReceptor)
            {
                VisualStateManager.GoToState(this, VisualStates.IsDropTargetState, true);
            }
        }

        public void DragDropExit(ToolbarCommand dataContext)
        {
            if (dataContext.CommandType == CommandType.MembraneReceptor)
            {
                VisualStateManager.GoToState(this, VisualStates.NotDropTargetState, true);
            }
        }

        public int SnapSize
        {
            get
            {
                // Don't snap
                // TODO - could snap instead of using module cell container
                return 0;
            }
        }

        public void ThumbnailDroping(ToolbarCommand dataContext, FrameworkElement cursor, Point cursorPosition)
        {
            if (dataContext.CommandType == CommandType.MembraneReceptor)
            {
                DragDropExit(dataContext);
                OnDropMembraneReceptor();
            }
        }

        private void OnDropMembraneReceptor()
        {
            var containerView = SilverlightVisualTreeHelper.TryFindParent<ContainerView>(this);

            var containerVM = (ContainerViewModel)containerView.DataContext;

            double positionX = Canvas.GetLeft(this);
            double positionY = Canvas.GetTop(this);

            // Adjust for the margin from the Site to the View
            positionX -= 7;
            positionY -= 10;

            containerVM.NewMembraneReceptor(positionX, positionY, this.Angle);
        }

        public MembranceReceptorView AddMembraneReceptor(object dataContext)
        {
            var memView = new MembranceReceptorView();
            memView.Angle = this.Angle;
            memView.DataContext = dataContext;

            memView.SetBinding(MembranceReceptorView.IsCheckedProperty, new Binding("IsChecked") { Mode = BindingMode.TwoWay, FallbackValue = false });

            this.SiteContent.Content = memView;

            return memView;
        }

        public MembranceReceptorView GetMembraneReceptor()
        {
            var memView = this.SiteContent.Content as MembranceReceptorView;
            return memView;
        }

        public void RemoveMembraneReceptor()
        {
            var memView = this.SiteContent.Content as MembranceReceptorView;
            if(memView != null)
            {
                memView.Dispose();
            }

            this.SiteContent.Content = null;
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="Angle"/> property.
        /// </summary>
        public int Angle
        {
            get { return (int)GetValue(AngleProperty); }
            set { SetValue(AngleProperty, value); }
        }

        /// <summary>
        /// The <see cref="AngleProperty" /> dependency property registered with the 
        /// Microsoft Presentation Foundation (WPF) property system.
        /// </summary>
        public static readonly DependencyProperty AngleProperty =
            DependencyProperty.Register("Angle", typeof(int), typeof(MembraneReceptorSite), new PropertyMetadata(0, OnAngleChanged));

        private static void OnAngleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((MembraneReceptorSite) d).OnThisAngleChanged(d, e);
        }

        private void OnThisAngleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }
   
    }
}