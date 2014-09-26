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
    public partial class LogWindow : ChildWindow
    {
        /// <summary>
        /// Creates a new <see cref="ErrorWindow"/> instance.
        /// </summary>
        protected LogWindow(string log)
        {
            InitializeComponent();
            LogTextBox.Text = log;
            this.Closed += (s, args) => Application.Current.RootVisual.SetValue(Control.IsEnabledProperty, true);
        }

        /// <summary>
        /// Creates a new Error Window given an error message.
        /// Current stack trace will be displayed if app is running under debug or on the local machine.
        /// </summary>
        /// <param name="message">The message to display.</param>
        public static void CreateNew(string log)
        {
            var window = new LogWindow(log);
            window.Show();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}