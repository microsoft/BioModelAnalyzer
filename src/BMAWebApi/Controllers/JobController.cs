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
using System.Web.Http;

namespace bma.client.Controllers
{
    public class JobController : ApiController
    {
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
            string basePath = Path.Combine(Environment.GetEnvironmentVariable("RoleRoot"), @"approot\bin");
            try
            {
                string result = JobsRunner.Job.RunToCompletion(Path.Combine(basePath, executableName), input, timeoutMs);
                return HttpResponses.Json(Request, result);
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
