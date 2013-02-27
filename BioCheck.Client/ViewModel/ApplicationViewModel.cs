﻿using System;
using System.ComponentModel;
using System.IO.IsolatedStorage;
using BioCheck.States;
using BioCheck.ViewModel.Cells;
using BioCheck.ViewModel.Factories;
using BioCheck.ViewModel.Models;
using System.Linq;
using Microsoft.Practices.Unity;
using MvvmFx.Common.Helpers;
using MvvmFx.Common.ViewModels;
using MvvmFx.Common.ViewModels.Behaviors.LoadingSaving;
using MvvmFx.Common.ViewModels.Behaviors.Messaging;
using MvvmFx.Common.ViewModels.Behaviors.Observable;
using MvvmFx.Common.ViewModels.Factories;

namespace BioCheck.ViewModel
{
    /// <summary>
    /// Singleton ViewModel for the application shell
    /// </summary>
    /// <remarks>
    /// Initialises the types in the UnityContainer, exposes the WebServices Context, and
    /// exposes the active model and variable and sub-viewmodels.
    /// </remarks>
    public class ApplicationViewModel : ObservableViewModel
    {
        private readonly UnityContainer container;
        private ModelViewModel activeModel;
        private VariableViewModel activeVariable;
        private ContainerViewModel activeContainer;

        private readonly MessengerService messenger;
        private ApplicationContext context;
        private ApplicationLog log;
        private ApplicationUser user;

        private readonly ZoomViewModel zoomViewModel;
        private readonly ToolbarViewModel toolbarViewModel;
        private readonly ApplicationSettings settings;
        private LibraryViewModel library;
        private bool isLoading;

        #region Singleton Constructor

        private static readonly ApplicationViewModel instance
            = new ApplicationViewModel();

        static ApplicationViewModel()
        {

        }

        /// <summary>
        /// Singleton Constructor 
        /// </summary>
        public ApplicationViewModel()
        {
            this.container = new UnityContainer();
            this.settings = new ApplicationSettings();

            this.log = new ApplicationLog();
            this.user = new ApplicationUser();

            this.zoomViewModel = new ZoomViewModel();
            this.toolbarViewModel = new ToolbarViewModel();

            this.messenger = new MessengerService();
        }

        /// <summary>
        /// Initialise the ApplicationViewModel.
        /// Registers the types in the inversion of control container.
        /// </summary>
        public void Init()
        {
            bool isOffline = true;
            bool isFactoryReset = false;

            if (isOffline)
            {
                if (isFactoryReset)
                {
                    IsolatedStorageHelper.Reset();
                }

                Container.RegisterType(typeof(IViewModelFactory<LibraryViewModel>), typeof(LibraryLocalFactory), new ContainerControlledLifetimeManager());
                Container.RegisterType(typeof(IViewModelFactory<ModelViewModel>), typeof(ModelLocalFactory), new ContainerControlledLifetimeManager());
                Container.RegisterType(typeof(IViewModelSaver<ModelViewModel>), typeof(ModelLocalSaver), new ContainerControlledLifetimeManager());
            }
            else
            {
                Container.RegisterType(typeof(IViewModelFactory<LibraryViewModel>), typeof(LibraryLocalFactory), new ContainerControlledLifetimeManager());
                // TODO - re-add online mode
                //   Container.RegisterType(typeof(IViewModelFactory<ModelViewModel>), typeof(ModelFactory), new ContainerControlledLifetimeManager());
                //   Container.RegisterType(typeof(IViewModelSaver<ModelViewModel>), typeof(ModelSaver), new ContainerControlledLifetimeManager());
            }
        }

        /// <summary>
        /// Loads this instance.
        /// </summary>
        public void Load()
        {
            this.IsLoading = true;

            this.context = new ApplicationContext();

            // Check Isolate Storage is available
            if (!IsolatedStorageFile.IsEnabled)
            {
                // Log the error to the Log web service
                ApplicationViewModel.Instance.Log.Error("Isolated Storage isn't enabled.", "IsolatedStorageFile.IsEnabled returned false.");

                // Show the error message
                ErrorWindow.CreateNew("Local Storage is not enabled in this browser. Please follow the instructions below to enable it.",
                    "This application uses Isolated Storage to store models locally. Isolated Storage is currently disabled in your browser. This could be because:" +
                    Environment.NewLine +
                    Environment.NewLine +
                    "- It is not enabled in the Application Storage tab of the Silverlight configuration dialog. Enable it by right-clicking and choosing Silverlight from the context menu, then check Enable Application Storage on the Application Storage tab." +
                    Environment.NewLine +
                    Environment.NewLine +
                    "- Your browser is set to private browsing mode. Please disable private browsing temporarily mode and reload." +
                    Environment.NewLine +
                    Environment.NewLine +
                    "- Your browser does not support Isolated Storage. Please reload in a different browser." +
                    Environment.NewLine +
                    Environment.NewLine +
                    @"If you cannot resolve this problem, please contact bma@microsoft.com." +
                    Environment.NewLine +
                    Environment.NewLine
                    );
                this.IsLoading = false;
                return;
            }

            // Load the Model Library
            var libraryFactory = this.Container.Resolve<IViewModelFactory<LibraryViewModel>>();
            this.Library = libraryFactory.Create();

            // Get the current (last active) model
            var currentModelName = this.settings.ActiveModel;
            var currentModelVM = this.library.Models.FirstOrDefault(mvm => mvm.Name == currentModelName);
            if (currentModelVM == null)
            {
                currentModelVM = this.library.Models[0];
            }

            this.library.SelectedModel = currentModelVM;
            this.library.OpenCommand.Execute();

            // Log the usuage session to the Log web service
            DispatcherHelper.DoubleBeginInvoke(() => this.log.Login());
        }

        /// <summary>
        /// Shuts down.
        /// </summary>
        public void ShutDown()
        {

        }

        /// <summary>
        /// Gets the value of the <see cref="instance"/> property.
        /// </summary>
        public static ApplicationViewModel Instance
        {
            get { return instance; }
        }

        #endregion

        /// <summary>
        /// Gets the container.
        /// </summary>
        public UnityContainer Container
        {
            get { return container; }
        }

        /// <summary>
        /// Gets the value of the <see cref="Settings"/> property.
        /// </summary>
        public ApplicationSettings Settings
        {
            get { return settings; }
        }

        /// <summary>
        /// Gets the context.
        /// </summary>
        public ApplicationContext Context
        {
            get { return this.context; }
        }

        /// <summary>
        /// Gets the log.
        /// </summary>
        public ApplicationLog Log
        {
            get { return this.log; }
        }

        /// <summary>
        /// Gets the user.
        /// </summary>
        public ApplicationUser User
        {
            get { return this.user; }
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="IsLoading"/> property.
        /// </summary>
        public bool IsLoading
        {
            get { return this.isLoading; }
            set
            {
                if (this.isLoading != value)
                {
                    this.isLoading = value;
                    OnPropertyChanged(() => IsLoading);
                }
            }
        }

        /// <summary>
        /// Gets the value of the <see cref="Library"/> property.
        /// </summary>
        public LibraryViewModel Library
        {
            get { return library; }
            private set
            {
                if (this.library != value)
                {
                    this.library = value;
                    OnPropertyChanged(() => Library);
                }
            }
        }

        /// <summary>
        /// Gets the value of the <see cref="ZoomViewModel"/> property.
        /// </summary>
        public ZoomViewModel ZoomViewModel
        {
            get { return this.zoomViewModel; }
        }

        /// <summary>
        /// Gets the toolbar view model.
        /// </summary>
        public ToolbarViewModel ToolbarViewModel
        {
            get { return this.toolbarViewModel; }
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="ActiveModel"/> property.
        /// </summary>
        public ModelViewModel ActiveModel
        {
            get { return this.activeModel; }
            set
            {
                if (this.activeModel != value)
                {
                    this.activeModel = value;
                    OnPropertyChanged(() => ActiveModel);
                    OnPropertyChanged(() => HasActiveModel);

                    if (this.activeModel == null)
                    {
                        this.settings.ActiveModel = ApplicationSettings.DefaultModel;
                        this.toolbarViewModel.IsSelectionActive = true;
                    }
                    else
                    {
                        this.settings.ActiveModel = this.activeModel.Name;
                    }

                    this.settings.Save();

                    this.toolbarViewModel.ShowToolbar = this.HasActiveModel;
                }
            }
        }

        /// <summary>
        /// Gets the value of the <see cref="HasActiveModel"/> property.
        /// </summary>
        public bool HasActiveModel
        {
            get { return this.activeModel != null; }
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="ActiveVariable"/> property.
        /// </summary>
        public VariableViewModel ActiveVariable
        {
            get { return this.activeVariable; }
            set
            {
                if (this.activeVariable != value)
                {
                    // Deselect the previous active variable
                    if (this.activeVariable != null)
                    {
                        this.activeVariable.IsChecked = false;
                    }

                    if (value != null)
                    {
                        this.ActiveContainer = null;
                    }

                    this.activeVariable = value;
                    OnPropertyChanged(() => ActiveVariable);
                    OnPropertyChanged(() => HasActiveVariable);
                    OnPropertyChanged(() => HasActiveObject);
                    OnPropertyChanged(() => EditingVariable);
                }
            }
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="ActiveContainer"/> property.
        /// </summary>
        public ContainerViewModel ActiveContainer
        {
            get { return this.activeContainer; }
            set
            {
                if (this.activeContainer != value)
                {
                    // Deselect the previous active container
                    if (this.activeContainer != null)
                    {
                        this.activeContainer.IsChecked = false;
                    }

                    if(value != null)
                    {
                        this.ActiveVariable = null;
                    }

                    this.activeContainer = value;
                    OnPropertyChanged(() => ActiveContainer);
                    OnPropertyChanged(() => HasActiveContainer);
                    OnPropertyChanged(() => HasActiveObject);
                    OnPropertyChanged(() => EditingContainer);
                }
            }
        }

        public EditVariableViewModel EditingVariable
        {
            get
            {
                if (this.activeVariable != null)
                {
                    return new EditVariableViewModel(this.activeVariable);
                }
                return null;
            }
        }

        public EditContainerViewModel EditingContainer
        {
            get
            {
                if (this.activeContainer != null)
                {
                    return new EditContainerViewModel(this.activeContainer);
                }
                return null;
            }
        }

        /// <summary>
        /// Gets the value of the <see cref="HasActiveVariable"/> property.
        /// </summary>
        public bool HasActiveVariable
        {
            get { return this.activeVariable != null; }
        }

        /// <summary>
        /// Gets the value of the <see cref="HasActiveContainer"/> property.
        /// </summary>
        public bool HasActiveContainer
        {
            get { return this.activeContainer != null; }
        }

        /// <summary>
        /// Gets the value of the <see cref="HasActiveObject"/> property.
        /// </summary>
        public bool HasActiveObject
        {
            get { return HasActiveVariable || HasActiveContainer; }
        }

        public void Publish<T>(MessageBase msg)
        {
            this.messenger.Publish<T>(msg);
        }

        public void Subscribe<T>(Action<T> action)
        {
            this.messenger.Subscribe(action);
        }
    }
}