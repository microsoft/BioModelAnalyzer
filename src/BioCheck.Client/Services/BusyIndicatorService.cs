using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using BioCheck.Controls;

namespace BioCheck.Services
{
    public interface IBusyIndicatorService
    {
        void Show(string message);
        void Show(string message, ICommand cancelCommand);
        void Close();
    }

    public class BusyIndicatorService : IBusyIndicatorService
    {
        private readonly BusyWindow busyWindow;

        private bool isShowing;

        public BusyIndicatorService()
        {
            busyWindow = new BusyWindow();
            busyWindow.Closed += (s, args) => Application.Current.RootVisual.SetValue(Control.IsEnabledProperty, true);
        }

        public void Show(string message)
        {
            busyWindow.BusyText = message;
            busyWindow.IsCancellable = false;

            if (!isShowing)
            {
                isShowing = true;
                busyWindow.Show();
            }

        }

        public void Show(string message, ICommand cancelCommand)
        {
            busyWindow.BusyText = message;
            busyWindow.IsCancellable = true;
            busyWindow.CancelCommand = cancelCommand;

            if (!isShowing)
            {
                isShowing = true;
                busyWindow.Show();
            }
        }

        public void Close()
        {
            if (isShowing)
            {
                isShowing = false;
                busyWindow.Close();
            }
        }
    }
}
