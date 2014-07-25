using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace BioCheck.Controls
{
    public partial class BusyWindow : ChildWindow
    {
        public BusyWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="BusyText"/> property.
        /// </summary>
        public string BusyText
        {
            get { return (string) GetValue(BusyTextProperty); }
            set { SetValue(BusyTextProperty, value); }
        }

        /// <summary>
        /// The <see cref="BusyTextProperty" /> dependency property registered with the 
        /// Microsoft Presentation Foundation (WPF) property system.
        /// </summary>
        public static readonly DependencyProperty BusyTextProperty =
            DependencyProperty.Register("BusyText", typeof (string), typeof (BusyWindow), new PropertyMetadata("Please wait...", OnBusyTextChanged));

        private static void OnBusyTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((BusyWindow) d).OnThisBusyTextChanged(d, e);
        }

        private void OnThisBusyTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }


        public bool IsCancellable
        {
            get { return (bool)GetValue(IsCancellableProperty); }
            set { SetValue(IsCancellableProperty, value); }
        }

        /// <summary>
        /// Identifies the IsCancellable dependency property.
        /// </summary>
        public static readonly DependencyProperty IsCancellableProperty = DependencyProperty.Register(
            "IsCancellable",
            typeof(bool),
            typeof(BusyWindow),
            new PropertyMetadata(false, new PropertyChangedCallback(OnIsCancellableChanged)));

        private static void OnIsCancellableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((BusyWindow)d).OnIsCancellableChanged(e);
        }

        protected virtual void OnIsCancellableChanged(DependencyPropertyChangedEventArgs e)
        {
        }

        /// <summary>
        /// Gets or sets a value indicating the style to use for the progress bar.
        /// </summary>
        public ICommand CancelCommand
        {
            get { return (ICommand)GetValue(CancelCommandProperty); }
            set { SetValue(CancelCommandProperty, value); }
        }

        /// <summary>
        /// Identifies the CancelCommand dependency property.
        /// </summary>
        public static readonly DependencyProperty CancelCommandProperty = DependencyProperty.Register(
            "CancelCommand",
            typeof(ICommand),
            typeof(BusyWindow),
            new PropertyMetadata(null));
    }
}
