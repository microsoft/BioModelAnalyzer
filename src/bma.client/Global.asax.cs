using Newtonsoft.Json.Linq;
using System;
using System.Configuration;
using System.IO;
using System.Web;
using System.Web.Http;

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
                var pathToVer = HttpContext.Current != null ? HttpContext.Current.Server.MapPath("/version.txt") : Path.Combine(System.Environment.CurrentDirectory, "version.txt");
                version = JObject.Parse(File.ReadAllText(pathToVer));
            } 
            catch
            {
                version = new JObject();
                version.Add("major", 0);
                version.Add("minor", 0);
                version.Add("build", 0);
            }

            AppSettingsReader asr = new AppSettingsReader();

            //Math service URL
            try { 
                version.Add("computeServiceUrl", (string)asr.GetValue("BackEndUrl", typeof(string)));
            }
            catch
            {
                version.Add("computeServiceUrl", "");
            }

            //OneDrive App ID
            try
            {
                version.Add("onedriveappid", (string)asr.GetValue("LiveAppId", typeof(string)));
            }
            catch
            {
                version.Add("onedriveappid", "");
            }

            //OneDrive redirect URL
            try
            {
                version.Add("onedriveredirecturl", (string)asr.GetValue("RedirectUrl", typeof(string)));
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
            
            GlobalConfiguration.Configuration.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}"                    
            );
        }        
    }
}