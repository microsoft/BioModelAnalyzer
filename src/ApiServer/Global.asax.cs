// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
using BMAWebApi;
using System;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Cors;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;

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


            var container = new UnityContainer();
            container.LoadConfiguration();
            container.RegisterInstance<System.Web.Http.Hosting.IHostBufferPolicySelector>(new System.Web.Http.WebHost.WebHostBufferPolicySelector());

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
