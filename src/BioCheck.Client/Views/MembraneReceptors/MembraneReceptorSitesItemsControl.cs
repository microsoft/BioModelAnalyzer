using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using BioCheck.Controls;

namespace BioCheck.Views.MembraneReceptors
{
    public class MembraneReceptorSitesItemsControl : ItemsControl
    {
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            var contentitem = element as FrameworkElement;

            contentitem.SetBinding(Canvas.LeftProperty, new Binding("Left"));

            contentitem.SetBinding(Canvas.TopProperty, new Binding("Top"));

            //contentitem.SetBinding(MembraneReceptorSite.AngleProperty, new Binding("Angle"));

            base.PrepareContainerForItemOverride(element, item);
        }
    }
}
