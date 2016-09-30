using BMAWebApi;
using Microsoft.Practices.Unity;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json.Linq;
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
using System.Web.Security;
using System.Web.SessionState;
using System.Xml.Linq;

namespace bma.client
{
    public class VersionController : ApiController
    {
        // POST api/Version
        public JObject Get()
        {
            JObject version;

            //Client version
            try { 
                version = JObject.Parse(File.ReadAllText(HttpContext.Current.Server.MapPath("/version.txt")));
            } 
            catch
            {
                version = new JObject();
                version.Add("major", 0);
                version.Add("minor", 0);
                version.Add("build", 0);
            }

            //Math service URL
            try { 
                version.Add("computeServiceUrl", RoleEnvironment.GetConfigurationSettingValue("ComputeServiceUrl"));
            }
            catch
            {
                version.Add("computeServiceUrl", "");
            }

            //OneDrive App ID
            try
            {
                version.Add("onedriveappid", RoleEnvironment.GetConfigurationSettingValue("OneDriveAppId"));
            }
            catch
            {
                version.Add("onedriveappid", "");
            }

            //OneDrive redirect URL
            try
            {
                version.Add("onedriveredirecturl", RoleEnvironment.GetConfigurationSettingValue("OneDriveRedirectUrl"));
            }
            catch
            {
                version.Add("onedriveredirecturl", "");
            }

            return version;
        }
    }

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
            GlobalConfiguration.Configuration.Routes.MapHttpRoute(
                name: "TestLRA",
                routeTemplate: "api/{controller}/{action}/{id}"//,
                //defaults: new { controller = "TestRLA" }
            );

            var container = new UnityContainer();
            IFailureLogger logger;
            if (RoleEnvironment.IsAvailable)
                logger = new FailureAzureLogger(
                            CloudStorageAccount.Parse(
                                RoleEnvironment.GetConfigurationSettingValue("StorageConnectionString")));
            else
                logger = new FailureTraceLogger();
            container.RegisterInstance<IFailureLogger>(logger);
            GlobalConfiguration.Configuration.DependencyResolver = new UnityResolver(container);
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