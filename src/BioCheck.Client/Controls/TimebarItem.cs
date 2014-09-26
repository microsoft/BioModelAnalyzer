using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using BioCheck.Services;
using MvvmFx.Common.Helpers;

namespace BioCheck.Controls
{
    public class TimebarItem : Control, DragDropService<TimebarCommand>.IDragSource
    {
        // Time edit
        private const int DefaultFixedWidth = 42;
        private const int DefaultFixedHeight = 36;

        private DragDropService<TimebarCommand> dragDropHelper;

        public TimebarItem()
        {
            this.DefaultStyleKey = typeof(TimebarItem);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            dragDropHelper = new DragDropService<TimebarCommand>(this);
            dragDropHelper.DraggingEnabledDistance = 50.0;          // Was 5.0

            this.MouseLeftButtonDown += TimebarItem_MouseLeftButtonDown;
        }

        private Point leftMouseButtonDown;

        void TimebarItem_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!this.IsCheckable)
                return;

            this.leftMouseButtonDown = e.GetPosition(this);
            this.MouseLeftButtonUp += TimebarItem_MouseLeftButtonUp;
        }

        void TimebarItem_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.MouseLeftButtonUp -= TimebarItem_MouseLeftButtonUp;
            var leftMouseButtonUp = e.GetPosition(this);

            if (leftMouseButtonDown == leftMouseButtonUp)
                this.IsChecked = !this.IsChecked;
        }

        private FrameworkElement dragCursor;

        public FrameworkElement DragCursor
        {
            get
            {
                if (dragCursor == null)
                {
                    if (this.CursorTemplate != null)
                    {
                        var cursorControl = new ContentControl();
                        cursorControl.ContentTemplate = this.CursorTemplate;
                        cursorControl.Content = this.Payload;
                        cursorControl.Opacity = 0.6;        // Shadow transparency
                        cursorControl.Width = this.FixedWidth;
                        cursorControl.Height = this.FixedHeight;

                        dragCursor = cursorControl;
                    }
                }
                return dragCursor;
            }
        }

        public void Dropped()
        {

        }

        public DataTemplate CursorTemplate { get; set; }

        /// <summary>
        /// Gets or sets the value of the <see cref="Content"/> property.
        /// </summary>
        public FrameworkElement Content
        {
            get { return (FrameworkElement)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        /// <summary>
        /// The <see cref="ContentProperty" /> dependency property registered with the 
        /// Microsoft Presentation Foundation (WPF) property system.
        /// </summary>
        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register("Content", typeof(FrameworkElement), typeof(TimebarItem), new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the value of the <see cref="Command"/> property.
        /// </summary>
        public TimebarCommand Command
        {
            get { return (TimebarCommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        /// <summary>
        /// The <see cref="CommandProperty" /> dependency property registered with the 
        /// Microsoft Presentation Foundation (WPF) property system.
        /// </summary>
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(TimebarCommand), typeof(TimebarItem), new PropertyMetadata(null));

        public TimebarCommand Payload
        {
            get { return Command; }
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="IsCheckable"/> property.
        /// </summary>
        public bool IsCheckable
        {
            get { return (bool)GetValue(IsCheckableProperty); }
            set { SetValue(IsCheckableProperty, value); }
        }

        /// <summary>
        /// The <see cref="IsCheckableProperty" /> dependency property registered with the 
        /// Microsoft Presentation Foundation (WPF) property system.
        /// </summary>
        public static readonly DependencyProperty IsCheckableProperty =
            DependencyProperty.Register("IsCheckable", typeof(bool), typeof(TimebarItem), new PropertyMetadata(false));

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
            DependencyProperty.Register("IsChecked", typeof(bool), typeof(TimebarItem), new PropertyMetadata(false, OnIsCheckedChanged));

        private static void OnIsCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TimebarItem)d).OnIsCheckedChanged(e);
        }

        private void OnIsCheckedChanged(DependencyPropertyChangedEventArgs e)
        {
            if (!this.IsCheckable)
                return;

            if (this.IsChecked)
            {
                VisualStateManager.GoToState(this, "Checked", true);                // Might cause error? Yes; needed the VisualStateManager in place, 
                                                                                    // or there was no state to go to.
            }
            else
            {
                VisualStateManager.GoToState(this, "Unchecked", true);
            }
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
            DependencyProperty.Register("FixedWidth", typeof(int), typeof(TimebarItem), new PropertyMetadata(DefaultFixedWidth));


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
            DependencyProperty.Register("FixedHeight", typeof(int), typeof(TimebarItem), new PropertyMetadata(DefaultFixedHeight));

        #endregion
    }
}
