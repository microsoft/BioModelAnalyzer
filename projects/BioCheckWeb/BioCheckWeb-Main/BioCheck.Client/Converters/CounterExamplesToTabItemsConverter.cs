using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using BioCheck.ViewModel.Proof;

namespace BioCheck.Converters
{
    /// <summary>
    /// Convert the collection of CounterExampleInfo objects on the ProofViewModel
    /// to corresponding Tab Items and Headers.
    /// </summary>
    public class CounterExamplesToTabItemsConverter : IValueConverter
    {
        public DataTemplate BifurcationHeaderTemplate { get; set; }
        public DataTemplate OscillationHeaderTemplate { get; set; }

        public DataTemplate BifurcationTabTemplate { get; set; }
        public DataTemplate OscillationTabTemplate { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var source = value as IEnumerable;
            if (source != null)
            {
                var tabItems = new List<TabItem>();

                foreach (object item in source)
                {
                    var counterExampleInfo = (CounterExampleInfo)item;

                    ContentControl content = null;
                    ContentControl header = null;

                    if (counterExampleInfo.Type == CounterExampleTypes.Bifurcation)
                    {
                        header = new ContentControl
                        {
                            Content = CounterExampleTypes.Bifurcation,
                            ContentTemplate = BifurcationHeaderTemplate,
                            Margin= new Thickness(10, 0, 10, 0),
                        };

                        content = new ContentControl
                                      {
                                          Content = counterExampleInfo,
                                          ContentTemplate = BifurcationTabTemplate
                                      };
                    }
                    else if (counterExampleInfo.Type == CounterExampleTypes.Oscillation)
                    {
                        header = new ContentControl
                        {
                            Content = CounterExampleTypes.Oscillation,
                            ContentTemplate = OscillationHeaderTemplate,
                            Margin= new Thickness(10, 0, 10, 0),
                        };

                        content = new ContentControl
                        {
                            Content = counterExampleInfo,
                            ContentTemplate = OscillationTabTemplate
                        };
                    }

                    var tabItem = new TabItem
                    {
                        DataContext = counterExampleInfo,
                        Header = header,
                        Content = content
                    };

                    tabItems.Add(tabItem);
                }

                return tabItems;
            }
            return null;
        }

        /// <summary>
        /// ConvertBack method is not supported
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("ConvertBack method is not supported");
        }

    }
}
