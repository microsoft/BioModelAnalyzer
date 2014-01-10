using System;
using System.Windows;
using System.Windows.Controls;
using BioCheck.Services;
using BioCheck.ViewModel;
using BioCheck.ViewModel.Cells;
using Microsoft.Practices.Unity;

namespace BioCheck.Views
{

    [TemplateVisualState(Name = VisualStates.CheckedState, GroupName = VisualStates.CheckedStatesGroup)]
    [TemplateVisualState(Name = VisualStates.UncheckedState, GroupName = VisualStates.CheckedStatesGroup)]
    [TemplateVisualState(Name = VisualStates.MouseOverState, GroupName = VisualStates.MouseOverStatesGroup)]
    [TemplateVisualState(Name = VisualStates.NormalState, GroupName = VisualStates.MouseOverStatesGroup)]
    [TemplateVisualState(Name = VisualStates.ClearStability, GroupName = VisualStates.StableStates)]
    [TemplateVisualState(Name = VisualStates.IsStableState, GroupName = VisualStates.StableStates)]
    [TemplateVisualState(Name = VisualStates.NotStableState, GroupName = VisualStates.StableStates)]
    [TemplatePart(Name = TemplateParts.LayoutRoot, Type = typeof(Grid))]
    public class MembranceReceptorView : Control,
                                         IRelationshipTarget,
                                         IDisposable
    {
        private struct TemplateParts
        {
            public const string LayoutRoot = "LayoutRoot";
        }

        private struct VisualStates
        {
            public const string CheckedStatesGroup = "CheckStates";
            public const string CheckedState = "Checked";
            public const string UncheckedState = "Unchecked";

            public const string MouseOverStatesGroup = "MouseOverStates";
            public const string MouseOverState = "MouseOver";
            public const string NormalState = "Normal";

            public const string StableStates = "StableStates";
            public const string ClearStability = "ClearStability";
            public const string IsStableState = "IsStableState";
            public const string NotStableState = "NotStableState";
        }

        private RelationshipClient relationshipClient;
        private Grid layoutRoot;
        private IContextBarService contextBarService;

        private const int DefaultFixedWidth = 36;
        private const int DefaultFixedHeight = 36;

        public MembranceReceptorView()
        {
            this.DefaultStyleKey = typeof(MembranceReceptorView);
            this.relationshipClient = new RelationshipClient(this);

            this.Loaded += MembranceReceptorView_Loaded;
        }

        void MembranceReceptorView_Loaded(object sender, RoutedEventArgs e)
        {
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.layoutRoot = this.GetTemplateChild(TemplateParts.LayoutRoot) as Grid;
            if (this.layoutRoot == null)
                throw new MissingFieldException(TemplateParts.LayoutRoot);
        }

        private void OnMouseDoubleClick()
        {
            // Cancel drawing relationship if they double click whilst drawing
            var toolbarVM = ApplicationViewModel.Instance.ToolbarViewModel;
            if (toolbarVM.IsRelationshipActive)
            {
                this.relationshipClient.Cancel();
            }

            // Check the variable double-clicked on and set it as the Active Variable
            this.IsChecked = true;
            ApplicationViewModel.Instance.ActiveVariable = this.DataContext as VariableViewModel;
        }

        protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                OnMouseDoubleClick();
            }

            base.OnMouseLeftButtonDown(e);
        }

        protected override void OnMouseRightButtonDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonDown(e);

            if (this.contextBarService == null)
            {
                this.contextBarService = ApplicationViewModel.Instance.Container.Resolve<IContextBarService>();
            }

            VisualStateManager.GoToState(this, VisualStates.CheckedState, true);
            this.contextBarService.Show(this.DataContext, e);
            this.contextBarService.Closed += contextBarService_Closed;
        }

        void contextBarService_Closed(object sender, EventArgs e)
        {
            this.contextBarService.Closed -= contextBarService_Closed;
            VisualStateManager.GoToState(this, VisualStates.UncheckedState, true);
        }

        protected override void OnMouseEnter(System.Windows.Input.MouseEventArgs e)
        {
            base.OnMouseEnter(e);

            VisualStateManager.GoToState(this, VisualStates.MouseOverState, true);
        }

        protected override void OnMouseLeave(System.Windows.Input.MouseEventArgs e)
        {
            base.OnMouseLeave(e);

            VisualStateManager.GoToState(this, VisualStates.NormalState, true);
        }

        #region Size

        /// <summary>
        /// Gets or sets the value of the <see cref="FixedWidth"/> property.
        /// </summary>
        public int FixedWidth
        {
            get { return (int)GetValue(FixedWidthProperty); }
            set { SetValue(FixedWidthProperty, value); }
        }

        /// <summary>
        /// The <see cref="FixedWidthProperty" /> dependency property registered with the 
        /// Microsoft Presentation Foundation (WPF) property system.
        /// </summary>
        public static readonly DependencyProperty FixedWidthProperty =
            DependencyProperty.Register("FixedWidth", typeof(int), typeof(MembranceReceptorView), new PropertyMetadata(DefaultFixedWidth));

        /// <summary>
        /// Gets or sets the value of the <see cref="FixedHeight"/> property.
        /// </summary>
        public int FixedHeight
        {
            get { return (int)GetValue(FixedHeightProperty); }
            set { SetValue(FixedHeightProperty, value); }
        }

        /// <summary>
        /// The <see cref="FixedHeightProperty" /> dependency property registered with the 
        /// Microsoft Presentation Foundation (WPF) property system.
        /// </summary>
        public static readonly DependencyProperty FixedHeightProperty =
            DependencyProperty.Register("FixedHeight", typeof(int), typeof(MembranceReceptorView), new PropertyMetadata(DefaultFixedHeight));

        #endregion

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
            DependencyProperty.Register("IsChecked", typeof(bool), typeof(MembranceReceptorView), new PropertyMetadata(false, OnIsCheckedChanged));

        private static void OnIsCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((MembranceReceptorView)d).OnThisIsCheckedChanged(d, e);
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
            DependencyProperty.Register("ShowStability", typeof(bool), typeof(MembranceReceptorView), new PropertyMetadata(false, OnShowStabilityChanged));

        private static void OnShowStabilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((MembranceReceptorView)d).OnThisShowStabilityChanged(d, e);
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
            DependencyProperty.Register("IsStable", typeof(bool), typeof(MembranceReceptorView), new PropertyMetadata(true, OnIsStableChanged));

        private static void OnIsStableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((MembranceReceptorView)d).OnThisIsStableChanged(d, e);
        }

        private void OnThisIsStableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SetStability();
        }

        private void SetStability()
        {
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

        #region Angle

        /// <summary>
        /// Gets or sets the value of the <see cref="Angle"/> property.
        /// </summary>
        public double Angle
        {
            get { return (double)GetValue(AngleProperty); }
            set { SetValue(AngleProperty, value); }
        }

        /// <summary>
        /// The <see cref="AngleProperty" /> dependency property registered with the 
        /// Microsoft Presentation Foundation (WPF) property system.
        /// </summary>
        public static readonly DependencyProperty AngleProperty =
            DependencyProperty.Register("Angle", typeof(double), typeof(MembranceReceptorView), new PropertyMetadata(0d, OnAngleChanged));

        private static void OnAngleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((MembranceReceptorView)d).OnThisAngleChanged(d, e);
        }

        private void OnThisAngleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

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

        ~MembranceReceptorView()
        {
            Dispose(false);
        }

        protected virtual void OnDispose()
        {
      
        }

        #endregion

    }
}
