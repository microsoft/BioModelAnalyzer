using System;
using System.Windows;
using System.Windows.Controls;

namespace BioCheck.Controls
{
    public class UniformGrid : Panel
    {

        public int Rows
        {
            get { return (int)GetValue(RowsProperty); }
            set { SetValue(RowsProperty, value); }
        }

        public static readonly DependencyProperty RowsProperty =
            DependencyProperty.Register("Rows", typeof(int), typeof(UniformGrid), new PropertyMetadata(0, new PropertyChangedCallback(PropertyChanged)));

        public int Columns
        {
            get { return (int)GetValue(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }

        public static readonly DependencyProperty ColumnsProperty =
            DependencyProperty.Register("Columns", typeof(int), typeof(UniformGrid), new PropertyMetadata(0, new PropertyChangedCallback(PropertyChanged)));


        public int FirstColumn
        {
            get { return (int)GetValue(FirstColumnProperty); }
            set { SetValue(FirstColumnProperty, value); }
        }

        public static readonly DependencyProperty FirstColumnProperty =
            DependencyProperty.Register("FirstColumn", typeof(int), typeof(UniformGrid), new PropertyMetadata(0, new PropertyChangedCallback(PropertyChanged)));


        #region Methods

        private static void PropertyChanged(DependencyObject s, DependencyPropertyChangedEventArgs args)
        {
            ((UniformGrid)s).Refresh();           
        }

        private void Refresh()
        {
            InvalidateMeasure();
            InvalidateArrange();
        }


        protected override Size ArrangeOverride(Size arrangeSize)
        {
            Rect finalRect = new Rect(0.0, 0.0, arrangeSize.Width / ((double) this._columns), arrangeSize.Height / ((double) this._rows));
            double width = finalRect.Width;
            double num2 = arrangeSize.Width - 1.0;
            finalRect.X += finalRect.Width * this.FirstColumn;
            foreach (UIElement element in base.Children)
            {
                element.Arrange(finalRect);
                if (element.Visibility != Visibility.Collapsed)
                {
                    finalRect.X += width;
                    if (finalRect.X >= num2)
                    {
                        finalRect.Y += finalRect.Height;
                        finalRect.X = 0.0;
                    }
                }
            }
            return arrangeSize;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            this.UpdateComputedValues();
            Size availableSize = new Size(constraint.Width / ((double) this._columns), constraint.Height / ((double) this._rows));
            double width = 0.0;
            double height = 0.0;
            int num3 = 0;
            int count = base.Children.Count;
            while (num3 < count)
            {
                UIElement element = base.Children[num3];
                element.Measure(availableSize);
                Size desiredSize = element.DesiredSize;
                if (width < desiredSize.Width)
                {
                    width = desiredSize.Width;
                }
                if (height < desiredSize.Height)
                {
                    height = desiredSize.Height;
                }
                num3++;
            }
            return new Size(width * this._columns, height * this._rows);
        }

        private void UpdateComputedValues()
        {
            this._columns = this.Columns;
            this._rows = this.Rows;
            if (this.FirstColumn >= this._columns)
            {
                this.FirstColumn = 0;
            }
            if ((this._rows == 0) || (this._columns == 0))
            {
                int num = 0;
                int num2 = 0;
                int count = base.Children.Count;
                while (num2 < count)
                {
                    UIElement element = base.Children[num2];
                    if (element.Visibility != Visibility.Collapsed)
                    {
                        num++;
                    }
                    num2++;
                }
                if (num == 0)
                {
                    num = 1;
                }
                if (this._rows == 0)
                {
                    if (this._columns > 0)
                    {
                        this._rows = ((num + this.FirstColumn) + (this._columns - 1)) / this._columns;
                    }
                    else
                    {
                        this._rows = (int) Math.Sqrt((double) num);
                        if ((this._rows * this._rows) < num)
                        {
                            this._rows++;
                        }
                        this._columns = this._rows;
                    }
                }
                else if (this._columns == 0)
                {
                    this._columns = (num + (this._rows - 1)) / this._rows;
                }
            }
        }

        #endregion

        private int _columns;
        private int _rows;

    }
}
