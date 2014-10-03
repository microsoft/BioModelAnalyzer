using System;
using System.Diagnostics;
using System.Net;
using System.Net.Browser;
using BioCheck.Helpers;
using BioCheck.Services;
using BioCheck.ViewModel;
using Microsoft.Practices.Unity;

namespace BioCheck
{
    using System.Windows;

    /// <summary>
    /// Main <see cref="Application"/> class.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Creates a new <see cref="App"/> instance.
        /// </summary>
        public App()
        {
            InitializeComponent();

            // Create a WebContext and add it to the ApplicationLifetimeObjects collection.
            // This will then be available as WebContext.Current.
            //  WebContext webContext = new WebContext();
            //   webContext.Authentication = new FormsAuthentication();
            //  //webContext.Authentication = new WindowsAuthentication();
            // this.ApplicationLifetimeObjects.Add(webContext);
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // http://social.msdn.microsoft.com/Forums/en-US/windowsazuretroubleshooting/thread/e26aaa88-672c-4888-b54d-6c29bb0329f4/
            // TODO - should i remove this?
            HttpWebRequest.RegisterPrefix("http://", WebRequestCreator.ClientHttp);
            HttpWebRequest.RegisterPrefix("https://", WebRequestCreator.ClientHttp);



            // This will enable you to bind controls in XAML to WebContext.Current properties.
            //      this.Resources.Add("WebContext", WebContext.Current);

            // This will automatically authenticate a user when using Windows authentication or when the user chose "Keep me signed in" on a previous login attempt.
            //  WebContext.Current.Authentication.LoadUser(this.Application_UserLoaded, null);


            // Show some UI to the user while LoadUser is in progress
            this.InitializeRootVisual(e);
        }

        /// <summary>
        /// Invoked when the <see cref="LoadUserOperation"/> completes.
        /// Use this event handler to switch from the "loading UI" you created in <see cref="InitializeRootVisual"/> to the "application UI".
        /// </summary>
        //private void Application_UserLoaded(LoadUserOperation operation)
        //{
        //}

        /// <summary>
        /// Initializes the <see cref="Application.RootVisual"/> property.
        /// The initial UI will be displayed before the LoadUser operation has completed.
        /// The LoadUser operation will cause user to be logged in automatically if using Windows authentication or if the user had selected the "Keep me signed in" option on a previous login.
        /// </summary>
        protected virtual void InitializeRootVisual(StartupEventArgs e)
        {
            // Initalize the application ViewModel
            ApplicationViewModel.Instance.Init();

            // Store the user's IP address
            // No IP address appears when running the Silverlight project
            // directly - but in that case we don't actually care about the
            // IP address anyway.
            string ipAddress;
            if (!e.InitParams.TryGetValue("IPAddress", out ipAddress) || string.IsNullOrWhiteSpace(ipAddress))
                ipAddress = "Unknown:" + Guid.NewGuid();
            ApplicationViewModel.Instance.User.IPAddress = ipAddress;
            string modelUrl;
            if (e.InitParams.TryGetValue("Model", out modelUrl) && !string.IsNullOrWhiteSpace(modelUrl))
            {
                var url = new Uri(System.Windows.Browser.HtmlPage.Document.DocumentUri, modelUrl);
                ApplicationViewModel.Instance.InitialModelUrl = url.AbsoluteUri;
            }

            // Register UI Services with the Unity container
            var container = ApplicationViewModel.Instance.Container;
            container.RegisterType(typeof(IBusyIndicatorService), typeof(BusyIndicatorService), new ContainerControlledLifetimeManager());
            container.RegisterType(typeof(IErrorWindowService), typeof(ErrorWindowService), new ContainerControlledLifetimeManager());
            container.RegisterType(typeof(ILogWindowService), typeof(LogWindowService), new ContainerControlledLifetimeManager());
            container.RegisterType(typeof(IInvalidModelWindowService), typeof(InvalidModelWindowService), new ContainerControlledLifetimeManager());
            container.RegisterType(typeof(IMessageWindowService), typeof(MessageWindowService), new ContainerControlledLifetimeManager());

            var mainPage = new Shell();

            container.RegisterInstance(typeof(IContextBarService), new ContextBarService(mainPage), new ContainerControlledLifetimeManager());
            container.RegisterInstance(typeof(IRelationshipService), new RelationshipService(mainPage), new ContainerControlledLifetimeManager());
            container.RegisterInstance(typeof(IProofWindowService), new ProofWindowService(mainPage), new ContainerControlledLifetimeManager());
            container.RegisterInstance(typeof(ISimulationWindowService), new SimulationWindowService(mainPage), new ContainerControlledLifetimeManager());
            container.RegisterInstance(typeof(IGraphWindowService), new GraphWindowService(mainPage), new ContainerControlledLifetimeManager());

            // Time edit
            container.RegisterInstance(typeof(ITimeWindowService), new TimeWindowService(mainPage), new ContainerControlledLifetimeManager());
            container.RegisterInstance(typeof(ISynthWindowService), new SynthWindowService(mainPage), new ContainerControlledLifetimeManager());
            container.RegisterInstance(typeof(ISCMWindowService), new SCMWindowService(mainPage), new ContainerControlledLifetimeManager());

            var busyIndicator = new BioCheck.Controls.BusyIndicator();
            busyIndicator.Content = mainPage;
            busyIndicator.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            busyIndicator.VerticalContentAlignment = VerticalAlignment.Stretch;

            this.RootVisual = busyIndicator;
        }

        /// <summary>
        /// Returns whether running with a debugger attached or with the server hosted on localhost.
        /// </summary>
        public static bool IsRunningUnderDebugOrLocalhost
        {
            get
            {
                if (Debugger.IsAttached)
                {
                    return true;
                }
                else
                {
                    string hostUrl = Application.Current.Host.Source.Host;
                    return hostUrl.Contains("::1") || hostUrl.Contains("localhost") || hostUrl.Contains("127.0.0.1");
                }
            }
        }

        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            // If the app is running outside of the debugger then report the exception using a ChildWindow control.
            if (!IsRunningUnderDebugOrLocalhost)
            {
                // NOTE: This will allow the application to continue running after an exception has been thrown but not handled. 
                // For production applications this error handling should be replaced with something that will report the error to the website and stop the application.
                e.Handled = true;
                ErrorWindow.CreateNew("There was an unhandled exception. Please note the error details ", e.ExceptionObject.ToString());

                // Log the error to the Log web service
                ApplicationViewModel.Instance.Log.Error("There was an unhandled exception", e.ExceptionObject.ToString());
           }
        }
    }
}
