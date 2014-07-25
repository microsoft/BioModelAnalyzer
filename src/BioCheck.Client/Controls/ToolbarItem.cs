using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using BioCheck.Services;
using MvvmFx.Common.Helpers;

namespace BioCheck.Controls
{
    public class ToolbarItem : Control, DragDropService<ToolbarCommand>.IDragSource
    {
        // TODO - Change it to a behavior

        private const int DefaultFixedWidth = 42;
        private const int DefaultFixedHeight = 36;

        private DragDropService<ToolbarCommand> dragDropHelper;

        public ToolbarItem()
        {
            this.DefaultStyleKey = typeof(ToolbarItem);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            dragDropHelper = new DragDropService<ToolbarCommand>(this);
            dragDropHelper.DraggingEnabledDistance = 5.0;

            this.MouseLeftButtonDown += ToolbarItem_MouseLeftButtonDown;
        }

        private Point leftMouseButtonDown;

        void ToolbarItem_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Treat these as radio buttons - ie, don't allow a second click to uncheck
            if (!this.IsCheckable || this.IsChecked)
                return;

            this.leftMouseButtonDown = e.GetPosition(this);
            this.MouseLeftButtonUp += ToolbarItem_MouseLeftButtonUp;
        }

        void ToolbarItem_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.MouseLeftButtonUp -= ToolbarItem_MouseLeftButtonUp;
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
                        cursorControl.Opacity = 0.6;
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
            DependencyProperty.Register("Content", typeof(FrameworkElement), typeof(ToolbarItem), new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the value of the <see cref="Command"/> property.
        /// </summary>
        public ToolbarCommand Command
        {
            get { return (ToolbarCommand) GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        /// <summary>
        /// The <see cref="CommandProperty" /> dependency property registered with the 
        /// Microsoft Presentation Foundation (WPF) property system.
        /// </summary>
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof (ToolbarCommand), typeof (ToolbarItem), new PropertyMetadata(null));
   
        public ToolbarCommand Payload
        {
            get { return Command; }
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="IsCheckable"/> property.
        /// </summary>
        public bool IsCheckable
        {
            get { return (bool) GetValue(IsCheckableProperty); }
            set { SetValue(IsCheckableProperty, value); }
        }

        /// <summary>
        /// The <see cref="IsCheckableProperty" /> dependency property registered with the 
        /// Microsoft Presentation Foundation (WPF) property system.
        /// </summary>
        public static readonly DependencyProperty IsCheckableProperty =
            DependencyProperty.Register("IsCheckable", typeof (bool), typeof (ToolbarItem), new PropertyMetadata(false));

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
            DependencyProperty.Register("IsChecked", typeof(bool), typeof(ToolbarItem), new PropertyMetadata(false, OnIsCheckedChanged));

        private static void OnIsCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ToolbarItem)d).OnIsCheckedChanged(e);
        }

        private void OnIsCheckedChanged(DependencyPropertyChangedEventArgs e)
        {
            if (!this.IsCheckable)
                return;

            if(this.IsChecked)
            {
                VisualStateManager.GoToState(this, "Checked", true);
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
            DependencyProperty.Register("FixedWidth", typeof(int), typeof(ToolbarItem), new PropertyMetadata(DefaultFixedWidth));


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
            DependencyProperty.Register("FixedHeight", typeof(int), typeof(ToolbarItem), new PropertyMetadata(DefaultFixedHeight));

        #endregion
    }
}
