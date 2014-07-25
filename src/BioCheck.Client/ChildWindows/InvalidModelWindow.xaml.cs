namespace BioCheck
{
    using System;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Controls;
    using System.Net;

    /// <summary>
    /// <see cref="ChildWindow"/> class that displays errors to the user.
    /// </summary>
    public partial class InvalidModelWindow : ChildWindow
    {
        /// <summary>
        /// Creates a new <see cref="ErrorWindow"/> instance.
        /// </summary>
        public InvalidModelWindow(string log)
        {
            InitializeComponent();
            LogTextBox.Text = log;
            this.Closed += (s, args) => Application.Current.RootVisual.SetValue(Control.IsEnabledProperty, true);
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}