using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using BioCheck.Services;
using BioCheck.ViewModel;
using Microsoft.Practices.Unity;

namespace BioCheck.Behaviors
{
    public class CloseContextBarBehavior : Behavior<ButtonBase>
    {
        private IContextBarService contextBarService;

        protected override void OnAttached()
        {
            AssociatedObject.Click += OnButtonClick;
            AssociatedObject.Unloaded += OnUnloaded;
        }

        private void OnUnloaded(object sender, RoutedEventArgs args)
        {
            Interaction.GetBehaviors(this.AssociatedObject).Remove(this);
        }

        private void OnButtonClick(object sender, RoutedEventArgs args)
        {
            if (this.contextBarService == null)
            {
                this.contextBarService = ApplicationViewModel.Instance.Container.Resolve<IContextBarService>();
            }

            this.contextBarService.Close();
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Click -= OnButtonClick;
            AssociatedObject.Unloaded -= OnUnloaded;
        }
    }
}
