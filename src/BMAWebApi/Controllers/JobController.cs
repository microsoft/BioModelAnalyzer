using BioCheckAnalyzerCommon;
using bma.client;
using BMAWebApi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace bma.client.Controllers
{
    public class JobController : ApiController
    {
        private static string basePath;

        static JobController()
        {
            var env = Environment.GetEnvironmentVariables();
            foreach (var key in env.Keys)
            {
                if ((string)key == "RoleRoot")
                {
                    basePath = Path.Combine(Environment.GetEnvironmentVariable("RoleRoot"), @"approot\bin");
                    break;
                }
            }            
            if (basePath == null)
                if(HttpContext.Current != null) // if runs as a part of Web Application
                    basePath = HttpContext.Current.Server.MapPath(@"~\bin");
                else
                    basePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location); 
        }

        private readonly string executableName;
        private readonly IFailureLogger faultLogger;

        public JobController(string executableName, IFailureLogger logger)
        {            
            if (executableName == null) throw new ArgumentNullException("executableName");
            if (logger == null) throw new ArgumentNullException("logger");
            this.executableName = executableName;
            this.faultLogger = logger;
        }

        protected async Task<HttpResponseMessage> ExecuteAsync(int timeoutMs)
        {
            var log = new DefaultLogService();
            string input = await Request.Content.ReadAsStringAsync();

            try
            {
                JobsRunner.JobResult result = JobsRunner.Job.RunToCompletion(Path.Combine(basePath, executableName), input, timeoutMs);

                if (result.Errors.Length > 0)
                {
                    var contents = new LogContents(null, result.Errors);
                    faultLogger.Add(DateTime.Now, typeof(JobController).Assembly.GetName().Version.ToString(), input, contents);
                }
                return HttpResponses.Json(Request, result.Content);
            }
            catch (System.TimeoutException ex)
            {
                RegisterException(faultLogger, log, input, ex);
                return HttpResponses.PlainText(Request, "Timeout while waiting for the execution to complete", HttpStatusCode.NoContent); // status 204 if timeout
            }
            catch (Exception ex)
            {
                RegisterException(faultLogger, log, input, ex);
                throw ex;
            }
        }

        private void RegisterException(IFailureLogger faultLogger, DefaultLogService log, string input, System.Exception ex)
        {
            log.LogError(ex.ToString());
            var version = typeof(JobController).Assembly.GetName().Version;
            faultLogger.Add(DateTime.Now, version.ToString(), input, log);
        }
    }
}
