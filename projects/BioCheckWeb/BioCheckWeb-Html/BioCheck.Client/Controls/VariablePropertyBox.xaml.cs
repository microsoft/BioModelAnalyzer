using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace BioCheck.Controls
{
    public partial class VariablePropertyBox : UserControl
    {
        private const int DefaultRangeFrom = 2;
        private const int DefaultRangeTo = 3;

        public VariablePropertyBox()
        {
            InitializeComponent();

            this.Loaded += new RoutedEventHandler(VariablePropertyBox_Loaded);
        }

        void VariablePropertyBox_Loaded(object sender, RoutedEventArgs e)
        {
           // this.KeywordsComboBox.SelectionChanged += new SelectionChangedEventHandler(keywordsComboBox_SelectionChanged);
            this.InputsComboBox.SelectionChanged += InputsComboBox_SelectionChanged;

        }


        void InputsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            if (cb.SelectedIndex > -1)
            {
                cb.SelectedIndex = -1;
                VisualStateManager.GoToState(cb, "ShowWatermark", false);
            }
            else
            {
                // VisualStateManager.GoToState(cb, "HideWatermark", false);
                VisualStateManager.GoToState(cb, "ShowWatermark", false);
            }
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="CaretPosition"/> property.
        /// </summary>
        public int CaretPosition
        {
            get { return (int) GetValue(CaretPositionProperty); }
            set { SetValue(CaretPositionProperty, value); }
        }

        /// <summary>
        /// The <see cref="CaretPositionProperty" /> dependency property registered with the 
        /// Microsoft Presentation Foundation (WPF) property system.
        /// </summary>
        public static readonly DependencyProperty CaretPositionProperty =
            DependencyProperty.Register("CaretPosition", typeof (int), typeof (VariablePropertyBox), new PropertyMetadata(0, OnCaretPositionChanged));

        private static void OnCaretPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
          ((VariablePropertyBox) d).OnCaretPositionChanged();
        }

        private bool isChangingCaretPosition;

        private void OnCaretPositionChanged()
        {
            if(!isChangingCaretPosition)
            {
                FormulaTextBox.Focus();
                FormulaTextBox.Select(this.CaretPosition, 0);
            }
        }

        private void FormulaTextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            isChangingCaretPosition = true;

            var caret = FormulaTextBox.SelectionStart;
           // Debug.WriteLine("SelectionChanged: " + caret);

            this.CaretPosition = caret;

            isChangingCaretPosition = false;
        }

        private void KeywordsListBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Debug.WriteLine("Click");

            if(e.ClickCount == 2)
            {
                Debug.WriteLine("DOUBLE CLICK!!!!!");
                
            }
        }
    }
}
