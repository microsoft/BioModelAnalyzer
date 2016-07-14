using BioCheckAnalyzerCommon;
using BioModelAnalyzer;
using BMAWebApi;
using Microsoft.FSharp.Core;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace bma.client.Controllers
{
    public class AnalyzeLTLSimulationController : ApiController
    {
        private readonly IFailureLogger faultLogger;

        public AnalyzeLTLSimulationController(IFailureLogger logger)
        {
            this.faultLogger = logger;
        }

        public async Task<HttpResponseMessage> Post()
        {
            var log = new DefaultLogService();
            string input = await Request.Content.ReadAsStringAsync();
            string basePath = Path.Combine(Environment.GetEnvironmentVariable("RoleRoot"), @"approot\bin");
            try
            {
                string result = JobsRunner.Job.RunToCompletion(Path.Combine(basePath, "SimulateLTL.exe"), input, 60000);
                return HttpResponses.Json(Request, result);
            }
            catch (System.TimeoutException ex)
            {
                RegisterException(log, input, ex);
                return Request.CreateResponse(HttpStatusCode.NoContent, new HttpError("Timeout while waiting for the check to complete"));
            }
            catch (Exception ex)
            {
                RegisterException(log, input, ex);
                throw ex;
            }
        }

        private void RegisterException(DefaultLogService log, string input, System.Exception ex)
        {
            //  azureLogService.Debug("Analyze Exception", ex.ToString());
            log.LogError(ex.ToString());
            var version = typeof(AnalyzeController).Assembly.GetName().Version;
            faultLogger.Add(DateTime.Now, version.ToString(), input, log);
        }
    }

}