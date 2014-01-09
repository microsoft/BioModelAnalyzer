namespace BioCheck
{
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// <see cref="ChildWindow"/> class that displays errors to the user.
    /// </summary>
    public partial class ImportErrorWIndow : ChildWindow
    {
        /// <summary>
        /// Creates a new <see cref="ErrorWindow"/> instance.
        /// </summary>
        public ImportErrorWIndow(string log)
        {
            InitializeComponent();
            LogTextBox.Text = log;
            this.Closed += (s, args) => Application.Current.RootVisual.SetValue(Control.IsEnabledProperty, true);
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}