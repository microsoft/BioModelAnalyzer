using BMAWebApi;
using Microsoft.Practices.Unity;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
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
            
            // Force controllers assembly to be loaded
            var assembly = typeof(bma.client.Controllers.AnalyzeController).Assembly;
            GlobalConfiguration.Configuration.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}"                    
            );

            var container = new UnityContainer();
            FailureAzureLogger logger = new FailureAzureLogger(
                CloudStorageAccount.Parse(
                    RoleEnvironment.GetConfigurationSettingValue("StorageConnectionString")));
            container.RegisterInstance<IFailureLogger>(logger);
            GlobalConfiguration.Configuration.DependencyResolver = new UnityResolver(container);
        }        
    }
}