using System;
using MvvmFx.Common.ViewModels.Behaviors.Observable;
using MvvmFx.Common.ViewModels.Commands;

namespace BioCheck.ViewModel
{
    /// <summary>
    /// ViewModel for the Zoom control
    /// </summary>
    public class ZoomViewModel : ObservableViewModel
    {
        private const int DefaultZoomLevel = 50;
        private const int ZoomIncrement = 6;

        private double zoomLevel;

        public ZoomViewModel()
        {
            this.ZoomLevel = DefaultZoomLevel;

            this.zoomInCommand = new ActionCommand(param => ZoomLevel = Math.Max(0, ZoomLevel - ZoomIncrement));
            this.zoomOutCommand = new ActionCommand(param => ZoomLevel = Math.Min(100, ZoomLevel + ZoomIncrement));

            // Register ZoomLevel as a mappable-property that allows other 
            // ViewModels to auto-bind to its value without being strongly
            // coupled to this view model.
            this.Messenger.RegisterProperty(this, () => ZoomLevel);
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="ZoomLevel"/> property.
        /// </summary>
        public double ZoomLevel
        {
            get { return this.zoomLevel; }
            set
            {
                if (this.zoomLevel != value)
                {
                    this.zoomLevel = value;
                    OnPropertyChanged(() => ZoomLevel);
                }
            }
        }

        private readonly ActionCommand zoomInCommand;

        /// <summary>
        /// Gets the value of the <see cref="ZoomInCommand"/> property.
        /// </summary>
        public ActionCommand ZoomInCommand
        {
            get { return this.zoomInCommand; }
        }

        private readonly ActionCommand zoomOutCommand;

        /// <summary>
        /// Gets the value of the <see cref="ZoomOutCommand"/> property.
        /// </summary>
        public ActionCommand ZoomOutCommand
        {
            get { return this.zoomOutCommand; }
        }
    }
}