using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BioCheck.Controls;

namespace BioCheck.Services
{
    /// <summary>
    /// Service for showing and closing the ContextBar
    /// </summary>
    public interface IContextBarService
    {
        void Show(object dataContext, MouseButtonEventArgs e);
        void Show(object dataContext, MouseButtonEventArgs e, string style);
        void Close();

        event EventHandler Closed;
    }

    public class ContextBarService : IContextBarService
    {
        private readonly Canvas canvas;

        private bool isShowing;

        public ContextBarService(Shell shell)
        {
            this.canvas = shell.PopupCanvas;
        }

        public event EventHandler Closed;

        public void Show(object dataContext, MouseButtonEventArgs e)
        {
            Close();

            isShowing = true;

            e.Handled = true;

            var mousePosition = e.GetPosition(canvas);

            double x = mousePosition.X + 5;
            double y = mousePosition.Y + 5;

            var toolbar = new ContextBar();
            Canvas.SetLeft(toolbar, x);
            Canvas.SetTop(toolbar, y);

            toolbar.DataContext = dataContext;

            canvas.Children.Add(toolbar);
        }

        public void Show(object dataContext, MouseButtonEventArgs e, string style)
        {
            Close();

            isShowing = true;

            e.Handled = true;

            var mousePosition = e.GetPosition(canvas);

            double x = mousePosition.X + 5;
            double y = mousePosition.Y + 5;

            var toolbar = new ContextBar();
            Canvas.SetLeft(toolbar, x);
            Canvas.SetTop(toolbar, y);

            toolbar.DataContext = dataContext;
            toolbar.Style = (Style)App.Current.Resources[style];

            canvas.Children.Add(toolbar);
        }

        public void Close()
        {
            if (isShowing)
            {
                isShowing = false;
                canvas.Children.Clear();

                var handler = this.Closed;
                if (handler != null)
                    handler(this, EventArgs.Empty);
            }
        }
    }
}
