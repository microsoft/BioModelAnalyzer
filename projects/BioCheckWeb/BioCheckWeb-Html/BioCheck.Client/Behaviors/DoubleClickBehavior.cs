using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Threading;

namespace BioCheck.Behaviors
{
    public class DoubleClickBehavior : Behavior<UIElement>
    {
        public ICommand Command { get; set; }

        private readonly DispatcherTimer timer;

        public DoubleClickBehavior()
        {
            timer = new DispatcherTimer
            {
                Interval = new TimeSpan(0, 0, 0, 0, 200)
            };

            timer.Tick += OnTimerTick;
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.MouseLeftButtonDown += OnMouseButtonDown;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.MouseLeftButtonDown -= OnMouseButtonDown;
            if (timer.IsEnabled)
                timer.Stop();
        }

        private void OnMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!timer.IsEnabled)
            {
                timer.Start();
                return;
            }

            timer.Stop();

            if (Command != null)
            {
                Command.Execute(null);
            }
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            timer.Stop();
        }
    }
}
