using bma.Cloud;
using BMAWebApi;
using Microsoft.Practices.Unity;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Security;
using System.Web.SessionState;
using System.Xml.Linq;

namespace bma.client
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            GlobalConfiguration.Configuration.Formatters.XmlFormatter.UseXmlSerializer = true;
            // http://stackoverflow.com/questions/9847564/how-do-i-get-asp-net-web-api-to-return-json-instead-of-xml-using-chrome
            GlobalConfiguration.Configuration.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));

            // Force controllers assembly to be loaded
            var assembly = typeof(bma.client.Controllers.AnalyzeController).Assembly;

            GlobalConfiguration.Configuration.Routes.MapHttpRoute(
                name: "LongRunningActionsSpecificApi",
                routeTemplate: "api/lra/{appId}/{action}",
                defaults: new { controller = "longrunningactionsspecific" },
                constraints: new { appId = @"[0-9A-Fa-f]{8}[-]?([0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}" } 
            );
            GlobalConfiguration.Configuration.Routes.MapHttpRoute(
                name: "LongRunningActionsApi",
                routeTemplate: "api/lra/{appId}",
                defaults: new { controller = "longrunningactions" },
                constraints: new { appId = @"[0-9A-Fa-f]{8}[-]?([0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}" }
            );
            GlobalConfiguration.Configuration.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}"                    
            );


            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue("StorageConnectionString"));

            var container = new UnityContainer();
            IFailureLogger logger;
            if (RoleEnvironment.IsAvailable)
                logger = new FailureAzureLogger(storageAccount);
            else
                logger = new FailureTraceLogger();
            container.RegisterInstance<IFailureLogger>(logger);

            IScheduler scheduler;
            string schedulerName = "ltlpolarity"; // todo: can differ for different controllers; use setter injection with name?
            int maxNumberOfQueues = 3; // todo: should take from settings table

            FairShareSchedulerSettings settings = new FairShareSchedulerSettings(storageAccount, maxNumberOfQueues, schedulerName);
            scheduler = new FairShareScheduler(settings);
            container.RegisterInstance<IScheduler>(scheduler);

            GlobalConfiguration.Configuration.DependencyResolver = new UnityResolver(container);

            var cors = new EnableCorsAttribute("*", "*", "*");
            GlobalConfiguration.Configuration.EnableCors(cors);
        }        
    }

    internal class FailureTraceLogger : IFailureLogger
    {
        public void Add(DateTime dateTime, string backEndVersion, object request, ILogContents contents)
        {
            Trace.WriteLine(String.Format("Backend {1} fails at {0}", dateTime, backEndVersion));
        }
    }
}