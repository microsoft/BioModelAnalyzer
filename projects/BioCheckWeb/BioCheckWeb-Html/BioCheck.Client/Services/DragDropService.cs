using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using MvvmFx.Common.ExtensionMethods;

namespace BioCheck.Services
{
    public interface IDragSource
    {
        FrameworkElement DragCursor { get; }
        object Payload { get; }

        void Dropped();
    }

    public interface IDropTarget
    {
        void DragDropEnter(object dataContext);
        void DragDropExit(object dataContext);

        int SnapSize { get; }

        void ThumbnailDroping(object dataContext, FrameworkElement cursor, Point cursorPosition);
    }

    public interface IDragDropService : IDisposable
    {
        void Cancel();
    }

    public class DragDropService<PayloadType> : IDragDropService, IDisposable
    {
        public interface IDragSource
        {
            FrameworkElement DragCursor { get; }
            PayloadType Payload { get; }

            void Dropped();
        }

        public interface IDropTarget
        {
            void DragDropEnter(PayloadType dataContext, MouseEventArgs mouseEvent);
            void DragDropExit(PayloadType dataContext);

            int SnapSize { get; }

            void ThumbnailDroping(PayloadType dataContext, FrameworkElement cursor, Point cursorPosition);
        }

        public double DraggingEnabledDistance { get; set; }

        private IDragSource dragSource;
        private Point? dragOrigin;
        private Popup popup;
        private bool dragging;

        private IDropTarget currentDropTarget;

        public DragDropService(IDragSource source)
        {
            // Asign the drag source
            this.dragSource = source;
            this.dragOrigin = dragOrigin;

            DraggingEnabledDistance = 10.0;

            // Create the popup
            popup = new Popup { IsOpen = false };
            popup.MouseMove += MoveMouse;
            popup.MouseLeftButtonUp += MouseUp;

            // Hook up ALL source event handlers
            HookupAllSourceHandlers(dragSource as UIElement);
        }

        public DragDropService(IDragSource source, Point? dragOrigin)
        {
            // Asign the drag source
            this.dragSource = source;
            this.dragOrigin = dragOrigin;

            DraggingEnabledDistance = 10.0;

            // Create the popup
            popup = new Popup { IsOpen = false };
            popup.MouseMove += MoveMouse;
            popup.MouseLeftButtonUp += MouseUp;

            // Hook up source event handlers
            HookupSourceHandlers(dragSource as UIElement);
        }

        private void HookupSourceHandlers(UIElement element)
        {
            if (element == null) return;

            element.MouseMove += MoveMouse;
            element.MouseLeftButtonUp += MouseCancel;
            element.MouseLeave += MouseCancel;
        }

        private void HookupAllSourceHandlers(UIElement element)
        {
            if (element == null) return;

            element.MouseLeftButtonDown += MouseDown;
            element.MouseMove += MoveMouse;
            element.MouseLeftButtonUp += MouseCancel;
            element.MouseLeave += MouseCancel;
        }

        private void MouseDown(object sender, MouseEventArgs mouseEvent)
        {
            dragOrigin = mouseEvent.AbsolutePosition();
        }

        private void MoveMouse(object sender, MouseEventArgs mouseEvent)
        {
            if (!dragging && dragOrigin.HasValue)
            {
                var dragSource = mouseEvent.AbsolutePosition().FindElementBelow<IDragSource>();
                if (dragSource == null)
                    return;

                if (mouseEvent.AbsolutePosition().Distance(dragOrigin.Value) > DraggingEnabledDistance)
                    BeginDragDrop(dragSource, mouseEvent.AbsolutePosition());

                return;
            }

            if (dragging)
            {
                //
                // allow drop spot to set the position.
                //

                var mouseLocation = mouseEvent.AbsolutePosition();

                double width = dragSource.DragCursor.ActualWidth > 0
                         ? dragSource.DragCursor.ActualWidth
                         : dragSource.DragCursor.Width;

                double height = dragSource.DragCursor.ActualHeight > 0
                                   ? dragSource.DragCursor.ActualHeight
                                   : dragSource.DragCursor.Height;

                popup.HorizontalOffset = mouseLocation.X - (width / 2);
                popup.VerticalOffset = mouseLocation.Y - (height / 2);

                // UpdateDropSpot(mouseEvent);

                var dropSpot = mouseLocation.FindElementBelow<IDropTarget>();

                if (dropSpot != null && currentDropTarget == null)
                {
                    EnterDropSpot(dropSpot, mouseEvent);
                }
                else if (dropSpot == null || dropSpot != currentDropTarget)
                {
                    ExitDropSpot();
                }
                else if (this.currentDropTarget != null)
                {
                    if (this.currentDropTarget.SnapSize > 0)
                    {
                        // Apply the snap size to snap the cursor to the drop grid

                        var mousePositionRelativeToTarget = mouseEvent.GetPosition(this.currentDropTarget as UIElement);

                        double xSnapIndex = Math.Round(mousePositionRelativeToTarget.X / this.currentDropTarget.SnapSize);
                        double ySnapIndex = Math.Round(mousePositionRelativeToTarget.Y / this.currentDropTarget.SnapSize);

                        double xSnapPosition = this.currentDropTarget.SnapSize * xSnapIndex;
                        double ySnapPosition = this.currentDropTarget.SnapSize * ySnapIndex;

                        var snapDeltaX = xSnapPosition - mousePositionRelativeToTarget.X;
                        var snapDeltaY = ySnapPosition - mousePositionRelativeToTarget.Y;

                        var snappedX = mouseLocation.X + snapDeltaX;
                        var snappedY = mouseLocation.Y + snapDeltaY;

                        var centreSnappedX = snappedX - (width / 2);
                        var centreSnappedY = snappedY - (height / 2);

                        popup.HorizontalOffset = centreSnappedX;
                        popup.VerticalOffset = centreSnappedY;
                    }
                }
            }
        }

        private void MouseUp(object sender, MouseEventArgs mouseEvent)
        {
            dragOrigin = null;
            popup.Cursor = Cursors.Arrow;

            if (!dragging)
                return;

            dragging = false;
            popup.ReleaseMouseCapture();
            popup.IsOpen = false;

            var mouseLocation = mouseEvent.AbsolutePosition();

            //var dropSpot = mouseEvent.AbsolutePosition().FindElementBelow<IDropTarget>();
            //if (dropSpot != null)
            //{
            if (this.currentDropTarget != null)
            {
                //  var imagePosition = new Point(popup.HorizontalOffset, popup.VerticalOffset);

                var mousePositionRelativeToTarget = mouseEvent.GetPosition(this.currentDropTarget as UIElement);

                double width = dragSource.DragCursor.ActualWidth > 0
                         ? dragSource.DragCursor.ActualWidth
                         : dragSource.DragCursor.Width;

                double height = dragSource.DragCursor.ActualHeight > 0
                                   ? dragSource.DragCursor.ActualHeight
                                   : dragSource.DragCursor.Height;

                //pointRelative.X = pointRelative.X - (width / 2);
                //pointRelative.Y = pointRelative.Y - (height / 2);

                double xSnapIndex = Math.Round(mousePositionRelativeToTarget.X / this.currentDropTarget.SnapSize);
                double ySnapIndex = Math.Round(mousePositionRelativeToTarget.Y / this.currentDropTarget.SnapSize);

                double xSnapPosition = this.currentDropTarget.SnapSize * xSnapIndex;
                double ySnapPosition = this.currentDropTarget.SnapSize * ySnapIndex;

                //var snapDeltaX = xSnapPosition - mousePositionRelativeToTarget.X;
                //var snapDeltaY = ySnapPosition - mousePositionRelativeToTarget.Y;

                //var snappedX = mouseLocation.X + snapDeltaX;
                //var snappedY = mouseLocation.Y + snapDeltaY;

                //var centreSnappedX = snappedX - (width / 2);
                //var centreSnappedY = snappedY - (height / 2);

                var centreSnappedX = xSnapPosition - (width / 2);
                var centreSnappedY = ySnapPosition - (height / 2);

                var droppedPosition = new Point(centreSnappedX, centreSnappedY);

                this.currentDropTarget.ThumbnailDroping(dragSource.Payload, dragSource.DragCursor, droppedPosition);

                this.dragSource.Dropped();
            }
        }

        private void BeginDragDrop(IDragSource dragSource,
                                   Point mouseLocation)
        {
            dragging = true;

            popup.Child = dragSource.DragCursor;
            popup.CaptureMouse();
            popup.UpdateLayout();

            double width = dragSource.DragCursor.ActualWidth > 0
                               ? dragSource.DragCursor.ActualWidth
                               : dragSource.DragCursor.Width;

            double height = dragSource.DragCursor.ActualHeight > 0
                               ? dragSource.DragCursor.ActualHeight
                               : dragSource.DragCursor.Height;

            popup.HorizontalOffset = mouseLocation.X - (width / 2);
            popup.VerticalOffset = mouseLocation.Y - (height / 2);

            popup.Cursor = Cursors.Hand;
            popup.IsOpen = true;
        }

        private void EnterDropSpot(IDropTarget dropTarget, MouseEventArgs mouseEvent)
        {
            currentDropTarget = dropTarget;
            currentDropTarget.DragDropEnter(dragSource.Payload, mouseEvent);
        }

        private void ExitDropSpot()
        {
            if (currentDropTarget != null)
            {
                currentDropTarget.DragDropExit(dragSource.Payload);
                currentDropTarget = null;
            }
        }

        private void MouseCancel(object sender, MouseEventArgs mouseEvent)
        {
            Cancel();
        }

        public void Cancel()
        {
            dragging = false;
            dragOrigin = null;
            popup.Cursor = Cursors.Arrow;
            popup.Child = null;
        }


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

        ~DragDropService()
        {
            Dispose(false);
        }

        protected virtual void OnDispose()
        {
            var element = dragSource as UIElement;

            if (element != null)
            {
                element.MouseLeftButtonDown -= MouseDown;
                element.MouseMove -= MoveMouse;
                element.MouseLeftButtonUp -= MouseCancel;
                element.MouseLeave -= MouseCancel;
            }

            if (popup != null)
            {
                popup.MouseMove -= MoveMouse;
                popup.MouseLeftButtonUp -= MouseUp;
                popup.Child = null;
            }
        }

        #endregion
    }
}
