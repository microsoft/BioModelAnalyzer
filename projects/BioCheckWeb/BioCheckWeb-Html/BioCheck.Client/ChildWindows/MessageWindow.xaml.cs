using BioCheck.Services;

namespace BioCheck
{
    using System;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Controls;
    using System.Net;

    /// <summary>
    /// <see cref="MessageWindow"/> class that displays messages to the user.
    /// </summary>
    public partial class MessageWindow : ChildWindow
    {
        public MessageResult MessageResult { get; private set; }

        /// <summary>
        /// Creates a new <see cref="MessageWindow"/> instance.
        /// </summary>
        public MessageWindow(string message) : this(message, MessageType.OKOnly)
        {
         
        }

        /// <summary>
        /// Creates a new <see cref="MessageWindow"/> instance.
        /// </summary>
        public MessageWindow(string message, MessageType type)
        {
            InitializeComponent();
            LogTextBox.Text = message;
            this.Closed += (s, args) => Application.Current.RootVisual.SetValue(Control.IsEnabledProperty, true);

            switch (type)
            {
                case MessageType.OKOnly:
                    this.OKButton.Visibility = Visibility.Visible;
                    this.YesButton.Visibility = Visibility.Collapsed;
                    this.NoButton.Visibility = Visibility.Collapsed;
                    this.CancelButton.Visibility = Visibility.Collapsed;
                    break;
                case MessageType.OKCancel:
                    this.OKButton.Visibility = Visibility.Visible;
                    this.YesButton.Visibility = Visibility.Collapsed;
                    this.NoButton.Visibility = Visibility.Collapsed;
                    this.CancelButton.Visibility = Visibility.Visible;
                    break;
                case MessageType.YesNo:
                    this.OKButton.Visibility = Visibility.Collapsed;
                    this.YesButton.Visibility = Visibility.Visible;
                    this.NoButton.Visibility = Visibility.Visible;
                    this.CancelButton.Visibility = Visibility.Collapsed;
                    break;
                case MessageType.YesCancel:
                    this.OKButton.Visibility = Visibility.Collapsed;
                    this.YesButton.Visibility = Visibility.Visible;
                    this.NoButton.Visibility = Visibility.Collapsed;
                    this.CancelButton.Visibility = Visibility.Visible;
                    break;
                case MessageType.YesNoCancel:
                    this.OKButton.Visibility = Visibility.Collapsed;
                    this.YesButton.Visibility = Visibility.Visible;
                    this.NoButton.Visibility = Visibility.Visible;
                    this.CancelButton.Visibility = Visibility.Visible;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("type");
            }
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.MessageResult = MessageResult.OK;
            this.Close();
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            this.MessageResult = MessageResult.Yes;
            this.Close();
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            this.MessageResult = MessageResult.No;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.MessageResult = MessageResult.Cancel;
            this.Close();
        }
    }
}