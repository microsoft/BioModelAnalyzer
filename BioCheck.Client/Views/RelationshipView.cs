using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using BioCheck.Controls;
using BioCheck.Controls.Arrows;
using BioCheck.Services;
using BioCheck.ViewModel;
using BioCheck.ViewModel.Cells;
using MvvmFx.Common.ExtensionMethods;
using Microsoft.Practices.Unity;

namespace BioCheck.Views
{
    [TemplateVisualState(Name = VisualStates.CheckedState, GroupName = VisualStates.CheckedStatesGroup)]
    [TemplateVisualState(Name = VisualStates.UncheckedState, GroupName = VisualStates.CheckedStatesGroup)]
    public class RelationshipView : LoopingArrowLine,
                                    IDisposable
    {
        private static Brush DefaultNormalFill = new SolidColorBrush(Colors.Black);
        private static Brush DefaultCheckedFill = new SolidColorBrush(Colors.Gray);

        private struct VisualStates
        {
            public const string CheckedStatesGroup = "CheckStates";
            public const string CheckedState = "Checked";
            public const string UncheckedState = "Unchecked";
        }

        private struct ArrowHeadAngles
        {
            public const double Flat = 91.1;
            public const double Arrow = 89.5;
        }

        private IContextBarService contextBarService;

        public RelationshipView()
        {
            this.MouseLeftButtonDown += RelationshipView_MouseLeftButtonDown;
            this.MouseLeftButtonUp += RelationshipView_MouseLeftButtonUp;
            this.MouseRightButtonDown += RelationshipView_MouseRightButtonDown;

            this.Cursor = Cursors.Hand;
        }

        public RelationshipView(bool isTemp)
        {

        }

        void RelationshipView_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (this.contextBarService == null)
                this.contextBarService = ApplicationViewModel.Instance.Container.Resolve<IContextBarService>();

            this.IsChecked = true;

            var contextBarVM = new RelationshipContextBarViewModel(this.DataContext as RelationshipViewModel);

            this.contextBarService.Show(contextBarVM, e);
            this.contextBarService.Closed += contextBarService_Closed;
        }

        void contextBarService_Closed(object sender, EventArgs e)
        {
            this.contextBarService.Closed -= contextBarService_Closed;
            this.IsChecked = false;
        }

        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the value of the <see cref="Type"/> property.
        /// </summary>
        public RelationshipTypes Type
        {
            get { return (RelationshipTypes)GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }

        /// <summary>
        /// The <see cref="TypeProperty" /> dependency property registered with the 
        /// Microsoft Presentation Foundation (WPF) property system.
        /// </summary>
        public static readonly DependencyProperty TypeProperty =
            DependencyProperty.Register("Type", typeof(RelationshipTypes), typeof(RelationshipView), new PropertyMetadata(RelationshipTypes.None, OnTypeChanged));

        private static void OnTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((RelationshipView)d).OnThisTypeChanged(d, e);
        }

        private void OnThisTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (this.Type == RelationshipTypes.Activator)
            {
                this.ArrowAngle = ArrowHeadAngles.Arrow;
            }
            else if (this.Type == RelationshipTypes.Inhibitor)
            {
                this.ArrowAngle = ArrowHeadAngles.Flat;
            }
        }

        #region IsChecked

        private TimeSpan clickTimeSpan = 250.Miliseconds();
        private DateTime clickStart;

        void RelationshipView_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (DateTime.Now - this.clickStart <= this.clickTimeSpan)
            {
                this.IsChecked = !this.IsChecked;
            }
        }

        void RelationshipView_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.clickStart = DateTime.Now;
        }

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
            DependencyProperty.Register("IsChecked", typeof(bool), typeof(RelationshipView), new PropertyMetadata(false, OnIsCheckedChanged));

        private static void OnIsCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((RelationshipView)d).OnThisIsCheckedChanged(d, e);
        }

        private void OnThisIsCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (this.IsChecked)
            {
                // VisualStateManager.GoToState(this, VisualStates.CheckedState, true);
                this.Stroke = CheckedFill;
                this.Fill = CheckedFill;

                this.StrokeThickness = 3;
            }
            else
            {
                // VisualStateManager.GoToState(this, VisualStates.UncheckedState, true);
                this.Stroke = NormalFill;
                this.Fill = NormalFill;

                this.StrokeThickness = 1;

            }
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="NormalFill"/> property.
        /// </summary>
        public Brush NormalFill
        {
            get { return (Brush)GetValue(NormalFillProperty); }
            set { SetValue(NormalFillProperty, value); }
        }

        /// <summary>
        /// The <see cref="NormalFillProperty" /> dependency property registered with the 
        /// Microsoft Presentation Foundation (WPF) property system.
        /// </summary>
        public static readonly DependencyProperty NormalFillProperty =
            DependencyProperty.Register("NormalFill", typeof(Brush), typeof(RelationshipView), new PropertyMetadata(DefaultNormalFill, OnNormalFillChanged));

        private static void OnNormalFillChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((RelationshipView)d).OnThisNormalFillChanged(d, e);
        }

        private void OnThisNormalFillChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }

        /// <summary>
        /// Gets or sets the value of the <see cref="CheckedFill"/> property.
        /// </summary>
        public Brush CheckedFill
        {
            get { return (Brush)GetValue(CheckedFillProperty); }
            set { SetValue(CheckedFillProperty, value); }
        }

        /// <summary>
        /// The <see cref="CheckedFillProperty" /> dependency property registered with the 
        /// Microsoft Presentation Foundation (WPF) property system.
        /// </summary>
        public static readonly DependencyProperty CheckedFillProperty =
            DependencyProperty.Register("CheckedFill", typeof(Brush), typeof(RelationshipView), new PropertyMetadata(DefaultCheckedFill, OnCheckedFillChanged));

        private static void OnCheckedFillChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((RelationshipView)d).OnThisCheckedFillChanged(d, e);
        }

        private void OnThisCheckedFillChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
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

        ~RelationshipView()
        {
            Dispose(false);
        }

        protected virtual void OnDispose()
        {

        }

        #endregion
    }
}
