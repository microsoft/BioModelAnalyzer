// Define this to disable panning
//#define NO_PANNING

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using BioCheck.Services;
using BioCheck.ViewModel;
using BioCheck.ViewModel.Cells;
using BioCheck.ViewModel.Editing;
using BioCheck.Views;
using MvvmFx.Common.Helpers;
using MvvmFx.Common.Helpers.WeakEventing;
using Microsoft.Practices.Unity;
using System.Diagnostics;

namespace BioCheck.Controls
{
    public partial class ContainerGrid : UserControl,
                                     CollectionSource<ContainerViewModel>.ICollectionPresenter,
                                     CollectionSource<RelationshipViewModel>.ICollectionPresenter,
                                     CollectionSource<VariableViewModel>.ICollectionPresenter,
                                     IDisposable
    {
        private IContextBarService contextBarService;
        private IRelationshipService relationshipService;

        private static int DefaultRows = 10;
        private static int DefaultColumns = 10;

        private static double DefaultTranslateX = -300;
        private static double DefaultTranslateY = -330;

        private CollectionSource<ContainerViewModel> containersSource;
        private CollectionSource<VariableViewModel> variablesSource;
        private CollectionSource<RelationshipViewModel> relationshipsSource;

        private Canvas containerCanvas;
        public Canvas arrowCanvas;

        private bool isLoaded = false;

        public ContainerGrid()
        {
            InitializeComponent();

            this.Loaded += ContainerGrid_Loaded;

            // setup wrapper for CollectionChanged
            this.containersSource = new CollectionSource<ContainerViewModel>(this);
            this.variablesSource = new CollectionSource<VariableViewModel>(this);
            this.relationshipsSource = new CollectionSource<RelationshipViewModel>(this);

            // Register the shortcurt key
            var shortcutKeyManager =
                new ShortcutKeysManager(this)
                    .Register(ApplicationViewModel.Instance.ToolbarViewModel.DeleteCommand, Key.Delete)
                    .Register(ApplicationViewModel.Instance.ToolbarViewModel.SaveCommand, Key.S, ModifierKeys.Control)
                    .Register(ApplicationViewModel.Instance.ToolbarViewModel.CutCommand, Key.X, ModifierKeys.Control)
                    .Register(ApplicationViewModel.Instance.ToolbarViewModel.CopyCommand, Key.C, ModifierKeys.Control)
                    .Register(ApplicationViewModel.Instance.ToolbarViewModel.PasteCommand, Key.V, ModifierKeys.Control)
                    .Register(ApplicationViewModel.Instance.ToolbarViewModel.UndoCommand, Key.Z, ModifierKeys.Control)
                    .Register(() =>
                    {
                        ApplicationViewModel.Instance.ToolbarViewModel.IsSelectionActive = true;
                        this.Cursor = Cursors.Arrow;
                        this.contextBarService.Close();
                        CopyPasteManager.Clear();
                    }, Key.Escape);
        }

        void ContainerGrid_Loaded(object sender, RoutedEventArgs e)
        {
            this.isLoaded = true;

            this.containerCanvas = this.ContainerCanvas;
            this.arrowCanvas = this.ArrowCanvas;

            this.contextBarService = ApplicationViewModel.Instance.Container.Resolve<IContextBarService>();
            this.relationshipService = ApplicationViewModel.Instance.Container.Resolve<IRelationshipService>();

            this.DrawCells();

            ApplicationViewModel.Instance.Container
                .Resolve<IRelationshipService>()
                .Init(this.arrowCanvas, this.RelationshipTemplate);
        }

        #region Rows/Columns

        public int Rows
        {
            get { return (int)GetValue(RowsProperty); }
            set { SetValue(RowsProperty, value); }
        }

        public static readonly DependencyProperty RowsProperty =
            DependencyProperty.Register("Rows", typeof(int), typeof(ContainerGrid), new PropertyMetadata(DefaultRows, new PropertyChangedCallback(RowsPropertyChanged)));

        public int Columns
        {
            get { return (int)GetValue(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }

        public static readonly DependencyProperty ColumnsProperty =
            DependencyProperty.Register("Columns", typeof(int), typeof(ContainerGrid), new PropertyMetadata(DefaultColumns, new PropertyChangedCallback(ColumnsPropertyChanged)));

        private static void RowsPropertyChanged(DependencyObject s, DependencyPropertyChangedEventArgs args)
        {
            ((ContainerGrid)s).RowsPropertyChanged(args);
        }

        private static void ColumnsPropertyChanged(DependencyObject s, DependencyPropertyChangedEventArgs args)
        {
            ((ContainerGrid)s).ColumnsPropertyChanged(args);
        }

        private void RowsPropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            if (!this.isLoaded)
            {
                return;
            }

            int oldNumber = Convert.ToInt32(args.OldValue);
            int newNumber = Convert.ToInt32(args.NewValue);

            if (oldNumber < newNumber)
            {
                // Add new rows
                var rows = this.Rows;
                var columns = this.Columns;

                for (int rowIndex = oldNumber; rowIndex < rows; rowIndex++)
                {
                    for (int columnIndex = 0; columnIndex < columns; columnIndex++)
                    {
                        AddContainerSite(rowIndex, columnIndex);
                    }
                }
            }
            else if (oldNumber > newNumber)
            {
                // Remove old rows

                var rows = this.Rows;

                var toRemove = this.containerCanvas.Children.OfType<ContainerSite>()
                    .Where(containerSite => containerSite.PositionY >= rows)
                    .ToList();

                foreach (var containerSite in toRemove)
                {
                    this.containerCanvas.Children.Remove(containerSite);
                }
            }

            LayoutRoot.Height = this.Rows * ApplicationSettings.CellHeight;
        }

        private void ColumnsPropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            if (!this.isLoaded)
            {
                return;
            }

            int oldNumber = Convert.ToInt32(args.OldValue);
            int newNumber = Convert.ToInt32(args.NewValue);

            if (oldNumber < newNumber)
            {
                // Add new columns
                var rows = this.Rows;
                var columns = this.Columns;

                for (int rowIndex = 0; rowIndex < rows; rowIndex++)
                {
                    for (int columnIndex = oldNumber; columnIndex < columns; columnIndex++)
                    {
                        AddContainerSite(rowIndex, columnIndex);
                    }
                }
            }
            else if (oldNumber > newNumber)
            {
                // Remove old columns
                var columns = this.Columns;

                var toRemove = this.containerCanvas.Children.OfType<ContainerSite>()
                    .Where(containerSite => containerSite.PositionX >= columns)
                    .ToList();

                foreach (var containerSite in toRemove)
                {
                    this.containerCanvas.Children.Remove(containerSite);
                }
            }

            LayoutRoot.Width = this.Columns * ApplicationSettings.CellWidth;
        }

        #endregion

        #region Draw Cells

        private void DrawCells()
        {
            var rows = this.Rows;
            var columns = this.Columns;

            for (int rowIndex = 0; rowIndex <= rows - 1; rowIndex++)
            {
                for (int columnIndex = 0; columnIndex <= columns - 1; columnIndex++)
                {
                    AddContainerSite(rowIndex, columnIndex);
                }
            }

            LayoutRoot.Height = this.Rows * ApplicationSettings.CellHeight;
            LayoutRoot.Width = this.Columns * ApplicationSettings.CellWidth;
        }

        private void AddContainerSite(int rowIndex, int columnIndex)
        {
            var cell = new ContainerSite();

            // Set the bindings to the ViewModel properties
            cell.SetBinding(ContainerSite.ShowStabilityProperty, new Binding("ShowStability") { FallbackValue = false });
            cell.SetBinding(ContainerSite.IsStableProperty, new Binding("IsStable") { FallbackValue = true });

            double left = columnIndex * ApplicationSettings.CellWidth;
            double top = rowIndex * ApplicationSettings.CellHeight;

            cell.Width = ApplicationSettings.CellWidth;
            cell.Height = ApplicationSettings.CellHeight;

            cell.PositionX = columnIndex;
            cell.PositionY = rowIndex;

            Canvas.SetLeft(cell, left);
            Canvas.SetTop(cell, top);

            if (rowIndex > 0)
            {
                cell.ShowTop = false;
            }

            if (columnIndex > 0)
            {
                cell.ShowLeft = false;
            }

            this.containerCanvas.Children.Add(cell);
        }

        #endregion

        #region ZoomLevel

        /// <summary>
        /// Gets or sets the value of the <see cref="ZoomLevel"/> property.
        /// </summary>
        public double ZoomLevel
        {
            get { return (double)GetValue(ZoomLevelProperty); }
            set { SetValue(ZoomLevelProperty, value); }
        }

        /// <summary>
        /// The <see cref="ZoomLevelProperty" /> dependency property registered with the 
        /// Microsoft Presentation Foundation (WPF) property system.
        /// </summary>
        public static readonly DependencyProperty ZoomLevelProperty =
            DependencyProperty.Register("ZoomLevel", typeof(double), typeof(ContainerGrid), new PropertyMetadata(50.0, OnZoomLevelChanged));

        private static void OnZoomLevelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ContainerGrid)d).OnThisZoomLevelChanged(d, e);
        }

        private void OnThisZoomLevelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            this.contextBarService.Close();

            double level = this.ZoomLevel;
            double scale = LevelToScale(level);

            // Quick debug test
            //var roundtrip = ScaleToLevel(scale);
            //Debug.WriteLine(string.Format("From {0} to {1} back to {2}", level, scale, roundtrip));

            scaleTransform.ScaleX = scale;
            scaleTransform.ScaleY = scale;

            var x = (Scroller.HorizontalOffset + this.currentPointerFraction.X * Scroller.ViewportWidth) / Scroller.ExtentWidth;
            var y = (Scroller.VerticalOffset + this.currentPointerFraction.Y * Scroller.ViewportHeight) / Scroller.ExtentHeight;

            zoomTransformer.ApplyLayoutTransform();
            this.UpdateLayout();

            double newHorizontal, newVertical;
            // TODO - will it ever be the case that NoPoint holds?
            if (this.currentPointerFraction != NoPoint)
            {
                // Keep current pointer location constant
                newHorizontal = x * Scroller.ExtentWidth - this.currentPointerFraction.X * Scroller.ViewportWidth;
                newVertical = y * Scroller.ExtentHeight - this.currentPointerFraction.Y * Scroller.ViewportHeight;
            }
            else
            {
                // Restore centre location
                newHorizontal = _relScrollX * Scroller.ExtentWidth - 0.5 * Scroller.ViewportWidth;
                newVertical = _relScrollY * Scroller.ExtentHeight - 0.5 * Scroller.ViewportHeight;
            }

            if (newHorizontal < 0) newHorizontal = 0;
            Scroller.ScrollToHorizontalOffset(newHorizontal);

            if (newVertical < 0) newVertical = 0;
            Scroller.ScrollToVerticalOffset(newVertical);
        }

        // TODO - work out why these zoom formulae were chosen (and the comments in the code seem to be misleading!)
        private static double LevelToScale(double level)
        {
            double scale = 1.0;

            if (level < 50)
            {
                // Zoom in
                // @0  = 2
                // @10 = 10  = (2 - (0.04 * 10)) = 1.6
                // @24 = 24

                var percent = 0.02 * level;
                scale = 2 - percent;
            }
            else if (level == 50.0)
            {
                // scale stays as 1
            }
            else if (level > 50)
            {
                // Zoom Out
                // 0      25     50
                // 2       1     0.5
                // 
                // @35 = 10 = 1 - (0.02 * 10) = 0.8
                // @45 = 20 = 1 - (0.02 * 20) = 0.6
                // % = 10/25 = 0.04 * 10

                var delta = level - 50;
                var percent = 0.01 * delta;
                scale = 1 - percent;
            }

            return scale;
        }

        private static double ScaleToLevel(double scale)
        {
            double level = 50.0;

            if (scale > 1.0)
            {
                var percent = 2.0 - scale;
                level = percent * 50.0;
            }
            else if (scale < 1.0)
            {
                var percent = 1.0 - scale;
                var delta = percent * 100.0;
                level = delta + 50.0;
            }
            // else level level as 50

            return level;
        }

        internal void ZoomToFit()
        {
            double totalWidth = this.LayoutRoot.Width, totalHeight = this.LayoutRoot.Height;
            double cellWidth = totalWidth / this.Columns, cellHeight = totalHeight / this.Rows;
            double displayWidth = Scroller.ActualWidth * 0.75, displayHeight = Scroller.ActualHeight * 0.75; // HACK - bit of margin
            int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;
            // Find occupied rectangle
            foreach (var c in this.containerCanvas.Children.OfType<ContainerSite>())
                if (c.DataContext != null || c.VariableCanvas.Children.FirstOrDefault() != null)
                {
                    if (c.PositionX < minX) minX = c.PositionX;
                    if (c.PositionX > maxX) maxX = c.PositionX;
                    if (c.PositionY < minY) minY = c.PositionY;
                    if (c.PositionY > maxY) maxY = c.PositionY;
                }
            // If nothing present, take the central 3x2 cells
            if (minX == int.MaxValue)
            {
                int cw = Math.Min(3, this.Columns), ch = Math.Min(2, this.Rows);
                minX = (this.Columns - cw) / 2; maxX = minX + cw - 1;
                minY = (this.Rows - ch) / 2; maxY = minY + ch - 1;
            }
            int w = maxX - minX + 1, h = maxY - minY + 1;
            double xScale = displayWidth / (w * cellWidth), yScale = displayHeight / (h * cellHeight);
            double scale = Math.Min(xScale, yScale);
            double xMargin = (displayWidth - w * cellWidth * scale) / 2, yMargin = (displayHeight - h * cellHeight * scale) / 2;
            double level = ScaleToLevel(scale);
            this.ZoomLevel = level;
            this.PanX = minX * cellWidth * scale;// - xMargin;
            this.PanY = minY * cellHeight * scale;// - yMargin;
        }
        #endregion

        #region Panning

        private Point mouseStartPosition;

        private void ContainerGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ApplicationViewModel.Instance
                    .Context.LeftMouseDown(e);

            //// Deselect the current variable
            //ApplicationViewModel.Instance.ActiveVariable = null;

            //this.contextBarService.Close();

            //var absoluteLocation = e.GetPosition(null);

            //var variableView = (from element in VisualTreeHelper.FindElementsInHostCoordinates(absoluteLocation, Application.Current.RootVisual)
            //                    where element is VariableView
            //                    select element as VariableView).FirstOrDefault();
            //if(variableView == null)
            //{
            //    var containerView = (from element in VisualTreeHelper.FindElementsInHostCoordinates(absoluteLocation, Application.Current.RootVisual)
            //                        where element is ContainerView
            //                         select element as ContainerView).FirstOrDefault();
            //    if (containerView != null)
            //    {


            //    }
            //}

#if !NO_PANNING
            var toolbarVM = ApplicationViewModel.Instance.ToolbarViewModel;
            // TODO - is this MouseDownIsHandled thing necessary or a leftover from earlier hackery?
            if (!toolbarVM.MouseDownIsHandled && e.OriginalSource is Grid)
            {
                ((UIElement)sender).CaptureMouse();
                this.mouseStartPosition = e.GetPosition(this);
                EnablePanning();
            }
#endif
        }

        private void EnablePanning()
        {
            this.panning = true;
            this.Cursor = Cursors.Hand;
        }

        private void DisablePanning()
        {
            this.panning = false;
            this.Cursor = null;
        }

        private bool panning;

        private void ContainerGrid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ApplicationViewModel.Instance
                   .Context.RightMouseUp(e);

            // ApplicationViewModel.Instance.ToolbarViewModel.IsSelectionActive = true;
            this.ReleaseMouseCapture();
            DisablePanning();
        }

        private void ContainerGrid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            ApplicationViewModel.Instance
                   .Context.RightMouseDown(e);

            //   this.contextBarService.Close();
            //  e.Handled = true;
            this.ReleaseMouseCapture();
            DisablePanning();
        }

        void ContainerGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ApplicationViewModel.Instance
                   .Context.LeftMouseUp(e);

               this.ReleaseMouseCapture();
               DisablePanning();
        }

        private void ContentPresenter_LostMouseCapture(object sender, MouseEventArgs e)
        {
            DisablePanning();
            this.currentPointerFraction = NoPoint;
        }

        void ContainerGrid_MouseMove(object sender, MouseEventArgs e)
        {
            ApplicationViewModel.Instance
                 .Context.MouseMove(e);

            var s = (ScrollContentPresenter)sender;
            var p = e.GetPosition(s);
            this.currentPointerFraction = new Point(p.X / s.ActualWidth, p.Y / s.ActualHeight);

            if (this.panning)
            {
                var mouseCurrentPosition = e.GetPosition(this);
                var deltaX = mouseCurrentPosition.X - mouseStartPosition.X;
                var deltaY = mouseCurrentPosition.Y - mouseStartPosition.Y;

                this.PanX -= deltaX;
                this.PanY -= deltaY;

                this.mouseStartPosition = mouseCurrentPosition;
            }
        }

        private void ContainerGrid_MouseEnter(object sender, MouseEventArgs e)
        {
            var s = (ScrollContentPresenter)sender;
            var p = e.GetPosition(s);
            this.currentPointerFraction = new Point(p.X / s.ActualWidth, p.Y / s.ActualHeight);
        }

        private void ContainerGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            this.currentPointerFraction = NoPoint;
        }

        private ScrollBar _horizontalScrollBar;
        private ScrollBar _verticalScrollBar;
        private double _relScrollX;
        private double _relScrollY;
        private Point currentPointerFraction;
        private static readonly Point NoPoint = new Point(-1, -1);

        private void HorizontalScrollBar_Loaded(object sender, RoutedEventArgs e)
        {
            // Register a handler any time a user scrolls horizontally
            _horizontalScrollBar = sender as ScrollBar;
            _horizontalScrollBar.Scroll += horizontalScrollBar_Scroll;
        }

        private void VerticalScrollBar_Loaded(object sender, RoutedEventArgs e)
        {
            // Register a handler any time a user scrolls vertically
            _verticalScrollBar = sender as ScrollBar;
            _verticalScrollBar.Scroll += verticalScrollBar_Scroll;
        }

        void horizontalScrollBar_Scroll(object sender, ScrollEventArgs e)
        {
            this.contextBarService.Close();

            SaveScrollerValues();
        }

        void verticalScrollBar_Scroll(object sender, ScrollEventArgs e)
        {
            this.contextBarService.Close();

            SaveScrollerValues();
        }

        private bool isChangingPan;

        /// <summary>
        /// Save the values of the scroller any time they are changed
        /// </summary>
        private void SaveScrollerValues()
        {
            // This concept is modified from the code at http://social.msdn.microsoft.com/Forums/en/wpf/thread/5202aae5-b2cc-4fc3-aa43-4541fcc856fb
            if (Scroller.ExtentWidth > 0)
            {
                _relScrollX = (Scroller.HorizontalOffset + 0.5 * Scroller.ViewportWidth) / Scroller.ExtentWidth;
            }
            if (Scroller.ExtentHeight > 0)
            {
                _relScrollY = (Scroller.VerticalOffset + 0.5 * Scroller.ViewportHeight) / Scroller.ExtentHeight;
            }

            isChangingPan = true;

            var modelVM = ApplicationViewModel.Instance.ActiveModel;
            if (modelVM != null)
            {
                modelVM.PanX = Scroller.HorizontalOffset;
                modelVM.PanY = Scroller.VerticalOffset;
            }
            isChangingPan = false;
        }

        private void ContainerGrid_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            e.Handled = true;

            ModifierKeys keys = Keyboard.Modifiers;
            bool shiftKey = (keys & ModifierKeys.Shift) != 0;
            bool controlKey = (keys & ModifierKeys.Control) != 0;

            // 2  to 1 to 0.5
            // 15    30   60
            double panUnit = 20;

            if (shiftKey)
            {
                // Pan Up/Down

                var verticalOffset = Scroller.VerticalOffset;

                if (e.Delta > 0)
                {
                    verticalOffset -= panUnit;
                }
                else if (e.Delta < 0)
                {
                    verticalOffset += panUnit;
                }

                Scroller.ScrollToVerticalOffset(verticalOffset);
                SaveScrollerValues();
            }
            else if (controlKey)
            {
                // Pan Left/Right

                var horizontalOffset = Scroller.HorizontalOffset;

                if (e.Delta > 0)
                {
                    horizontalOffset -= panUnit;
                }
                else if (e.Delta < 0)
                {
                    horizontalOffset += panUnit;
                }

                Scroller.ScrollToHorizontalOffset(horizontalOffset);
                SaveScrollerValues();
            }
            else
            {
                // Zoom
                var zoomVM = ApplicationViewModel.Instance.ZoomViewModel;
                if (e.Delta > 0)
                {
                    zoomVM.ZoomInCommand.Execute();
                }
                else if (e.Delta < 0)
                {
                    zoomVM.ZoomOutCommand.Execute();
                }
            }
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="PanX"/> property.
        /// </summary>
        public double PanX
        {
            get { return (double)GetValue(PanXProperty); }
            set { SetValue(PanXProperty, value); }
        }

        /// <summary>
        /// The <see cref="PanXProperty" /> dependency property registered with the 
        /// Microsoft Presentation Foundation (WPF) property system.
        /// </summary>
        public static readonly DependencyProperty PanXProperty =
            DependencyProperty.Register("PanX", typeof(double), typeof(ContainerGrid), new PropertyMetadata(DefaultTranslateX, OnPanXChanged));

        private static void OnPanXChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ContainerGrid)d).OnThisPanXChanged(d, e);
        }

        private void OnThisPanXChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!isChangingPan)
            {
                // Check if the new PanX value is within the bounds of 0 to ExtentWidth
                var panX = this.PanX;
                if (panX < 0)
                {
                    // panX = (0.5 * Scroller.ExtentWidth) - (0.5 * Scroller.ViewportWidth);
                    this.PanX = 0;
                }
                else if (panX > Scroller.ExtentWidth)
                {
                    panX = Scroller.ExtentWidth - Scroller.ViewportWidth;
                    this.PanX = panX;
                }
                else
                {
                    var horizontalOffset = this.PanX;
                    Scroller.ScrollToHorizontalOffset(horizontalOffset);
                    this.Dispatcher.BeginInvoke(SaveScrollerValues);
                }
            }
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="PanY"/> property.
        /// </summary>
        public double PanY
        {
            get { return (double)GetValue(PanYProperty); }
            set { SetValue(PanYProperty, value); }
        }

        /// <summary>
        /// The <see cref="PanYProperty" /> dependency property registered with the 
        /// Microsoft Presentation Foundation (WPF) property system.
        /// </summary>
        public static readonly DependencyProperty PanYProperty =
            DependencyProperty.Register("PanY", typeof(double), typeof(ContainerGrid), new PropertyMetadata(DefaultTranslateY, OnPanYChanged));

        private static void OnPanYChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ContainerGrid)d).OnThisPanYChanged(d, e);
        }

        private void OnThisPanYChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!isChangingPan)
            {
                var panY = this.PanY;
                if (panY < 0)
                {
                    // panY = (0.5 * Scroller.ExtentHeight) - (0.5 * Scroller.ViewportHeight);
                    this.PanY = 0;
                }
                else if (panY > Scroller.ExtentHeight)
                {
                    panY = Scroller.ExtentHeight - Scroller.ViewportHeight;
                    this.PanY = panY;
                }
                else
                {
                    var verticalOffset = this.PanY;
                    Scroller.ScrollToVerticalOffset(verticalOffset);
                    this.Dispatcher.BeginInvoke(SaveScrollerValues);
                }
            }
        }

        #endregion

        #region ContainersSource

        public IEnumerable ContainersSource
        {
            get { return (IEnumerable)GetValue(ContainersSourceProperty); }
            set { SetValue(ContainersSourceProperty, value); }
        }

        public static readonly DependencyProperty ContainersSourceProperty =
            CollectionSource<ContainerViewModel>.Register("ContainersSource", typeof(ContainerGrid));

        public void Reset(ContainerViewModel item)
        {
            foreach (var containerSite in this.containerCanvas.Children.OfType<ContainerSite>())
            {
                containerSite.Reset();
            }

            this.CellCanvas.Children.Clear();
        }

        public void AddItem(ContainerViewModel containerVM)
        {
            if (!this.isLoaded)
            {
                return;
            }

            var containerView = new ContainerView();
            containerView.DataContext = containerVM;

            this.CellCanvas.Children.Add(containerView);

            var containerSites = GetContainerSites(containerVM.Size, containerVM.PositionX, containerVM.PositionY);
            containerSites.ForEach(site => site.DataContext = containerVM);

            // Call OnSizeChanged to reset the Clip region and positions of the Membrane Receptor Sites.
            // Use the Dispatcher to invoke it as a background task so that the controls are all drawn when it runs,
            // and therefore have their final Canvas.Left and Canvas.Top positions
            this.Dispatcher.BeginInvoke(containerView.OnSizeChanged);
        }

        public void RemoveItem(ContainerViewModel containerVM)
        {
            if (!this.isLoaded)
            {
                return;
            }

            var containerSites = GetContainerSites(containerVM.Size, containerVM.PositionX, containerVM.PositionY);
            containerSites.ForEach(site => site.DataContext = null);

            var containerView = this.CellCanvas.Children
                      .OfType<ContainerView>()
                      .FirstOrDefault(cv => cv.DataContext == containerVM);

            if (containerView != null)
            {
                containerView.Dispose();
                this.CellCanvas.Children.Remove(containerView);
            }
        }

        public ContainerSite GetContainerSite(int positionX, int positionY)
        {
            return this.containerCanvas.Children
                        .OfType<ContainerSite>()
                        .FirstOrDefault(containerSite => containerSite.PositionX == positionX
                                                      && containerSite.PositionY == positionY);
        }

        public List<ContainerSite> GetContainerSites(ContainerSizeTypes size, int positionX, int positionY)
        {
            var sites = new List<ContainerSite>();

            sites.Add(GetContainerSite(positionX, positionY));

            if (size == ContainerSizeTypes.Two)
            {
                sites.Add(GetContainerSite(positionX + 1, positionY));
                sites.Add(GetContainerSite(positionX, positionY + 1));
                sites.Add(GetContainerSite(positionX + 1, positionY + 1));
            }
            else if (size == ContainerSizeTypes.Three)
            {
                // Treat this one as the centre cell and get the 8 surrounding it
                //sites.Add(GetContainerSite(positionX - 1, positionY - 1));
                //sites.Add(GetContainerSite(positionX, positionY - 1));
                //sites.Add(GetContainerSite(positionX + 1, positionY - 1));
                //sites.Add(GetContainerSite(positionX - 1, positionY));
                //sites.Add(GetContainerSite(positionX + 1, positionY));
                //sites.Add(GetContainerSite(positionX - 1, positionY + 1));
                //sites.Add(GetContainerSite(positionX, positionY + 1));
                //sites.Add(GetContainerSite(positionX + 1, positionY + 1));

                // Treat this one as the top left cell and get the 8 to the left and below it
                sites.Add(GetContainerSite(positionX + 1, positionY));
                sites.Add(GetContainerSite(positionX + 2, positionY));

                sites.Add(GetContainerSite(positionX, positionY + 1));
                sites.Add(GetContainerSite(positionX + 1, positionY + 1));
                sites.Add(GetContainerSite(positionX + 2, positionY + 1));

                sites.Add(GetContainerSite(positionX, positionY + 2));
                sites.Add(GetContainerSite(positionX + 1, positionY + 2));
                sites.Add(GetContainerSite(positionX + 2, positionY + 2));
            }

            return sites;
        }

        #endregion

        #region VariablesSource

        public IEnumerable VariablesSource
        {
            get { return (IEnumerable)GetValue(VariablesSourceProperty); }
            set { SetValue(VariablesSourceProperty, value); }
        }

        public static readonly DependencyProperty VariablesSourceProperty =
            CollectionSource<VariableViewModel>.Register("VariablesSource", typeof(ContainerGrid));

        public void Reset(VariableViewModel item)
        {
            if (!this.isLoaded)
            {
                return;
            }
        }

        public void AddItem(VariableViewModel variableVM)
        {
            if (!this.isLoaded)
                return;

            if (variableVM.Type == VariableTypes.Constant)
            {
                var containerSite = GetContainerSite(variableVM.CellX, variableVM.CellY);
                if (containerSite != null)
                {
                    var constantVariableView = containerSite.AddConstantVariable(variableVM);
                }
            }
        }

        public void RemoveItem(VariableViewModel variableVM)
        {
            if (variableVM.Type == VariableTypes.Constant)
            {
                var containerSite = GetContainerSite(variableVM.CellX, variableVM.CellY);
                if (containerSite != null)
                {
                    var constantVariableView = containerSite.GetConstantVariableView(variableVM);
                    containerSite.RemoveConstantVariable(variableVM);
                }
            }
        }

        #endregion

        #region RelationshipsSource

        public IEnumerable RelationshipsSource
        {
            get { return (IEnumerable)GetValue(RelationshipsSourceProperty); }
            set { SetValue(RelationshipsSourceProperty, value); }
        }

        public static readonly DependencyProperty RelationshipsSourceProperty =
            CollectionSource<RelationshipViewModel>.Register("RelationshipsSource", typeof(ContainerGrid));

        public void Reset(RelationshipViewModel item)
        {
            if (!this.isLoaded)
            {
                return;
            }
            this.relationshipService.RemoveRelationships();
        }

        public void AddItem(RelationshipViewModel relationshipVM)
        {
            if (!this.isLoaded)
            {
                return;
            }

            this.relationshipService.AddRelationship(relationshipVM);
        }

        public void RemoveItem(RelationshipViewModel relationshipVM)
        {
            this.relationshipService.RemoveRelationship(relationshipVM);
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="RelationshipTemplate"/> property.
        /// </summary>
        public DataTemplate RelationshipTemplate
        {
            get { return (DataTemplate)GetValue(RelationshipTemplateProperty); }
            set { SetValue(RelationshipTemplateProperty, value); }
        }

        /// <summary>
        /// The <see cref="RelationshipTemplateProperty" /> dependency property registered with the 
        /// Silverlight property system.
        /// </summary>
        public static readonly DependencyProperty RelationshipTemplateProperty =
            DependencyProperty.Register("RelationshipTemplate", typeof(DataTemplate), typeof(ContainerGrid), new PropertyMetadata(null));

        #endregion

        #region IDisposable Members

        private bool disposed;

        /// <summary>
        /// The Dispose method
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            // Use SupressFinalize in case a subclass
            // of this type implements a finalizer.
            //TODO GC.SuppressFinalize(this);
        }

        /// <summary>
        /// The protected virtual dispose that removes the handlers
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            // If you need thread safety, use a lock around these 
            // operations, as well as in your methods that use the resource.
            if (!disposed)
            {
                if (disposing)
                {
                    OnDispose();
                }

                // Indicate that the instance has been disposed.
                disposed = true;
            }
        }

        ~ContainerGrid()
        {
            Dispose(false);
        }

        protected virtual void OnDispose()
        {
            this.relationshipsSource.Dispose();
            this.relationshipsSource = null;
        }

        #endregion
    }
}
