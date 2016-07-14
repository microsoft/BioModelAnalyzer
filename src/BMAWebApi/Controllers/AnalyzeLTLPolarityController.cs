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
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace bma.client.Controllers
{
    public enum LTLStatus
    {
        False,
        True,
        Unknown
    }

    public class LTLAnalysisResult
    {
        public LTLStatus Status { get; set; }

        /// <summary>Additional error information if status is nor Stabilizing neither NonStabilizing</summary>
        [XmlIgnore]
        public string Error { get; set; }

        [XmlElement("Tick", Type = typeof(Tick))]
        public Tick[] Ticks { get; set; }

        public string[] ErrorMessages { get; set; }

        public string[] DebugMessages { get; set; }

        [XmlElement("Loop", Type = typeof(int))]
        public int Loop { get; set; }
    }

    public class LTLPolarityAnalysisInputDTO : Model
    {
        [XmlIgnore]
        public bool EnableLogging { get; set; }
        public string Formula { get; set; }

        public string Number_of_steps { get; set; }

        public LTLStatus Polarity { get; set; }
    }

    public class AnalyzeLTLPolarityController : ApiController
    {
        private readonly IFailureLogger faultLogger;

        public AnalyzeLTLPolarityController(IFailureLogger logger)
        {
            this.faultLogger = logger;
        }

        private static HttpContent Json(string content)
        {
            return new StringContent(content, System.Text.Encoding.UTF8, "application/json");
        }

        // POST api/AnalyzeLTL
        public async Task<HttpResponseMessage> Post()
        {
            var log = new DefaultLogService();
            string input = await Request.Content.ReadAsStringAsync();
            string basePath = Path.Combine(Environment.GetEnvironmentVariable("RoleRoot"), @"approot\bin");
            try
            {
                string result = JobsRunner.Job.RunToCompletion(Path.Combine(basePath, "AnalyzeLTL.exe"), input, 60000);

                var response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = Json(result);
                return response;
            }
            catch (System.TimeoutException ex)
            {
                RegisterException(log, input, ex);
                return Request.CreateResponse(HttpStatusCode.NoContent, new HttpError("Timeout while waiting for the check to complete"));
            }
            catch (Exception ex)
            {
                //  azureLogService.Debug("Analyze Exception", ex.ToString());
                log.LogError(ex.ToString());
                var version = typeof(AnalyzeController).Assembly.GetName().Version;
                faultLogger.Add(DateTime.Now, version.ToString(), input, log);
                throw ex;
            }
        }

        private void RegisterException(DefaultLogService log, string input, System.TimeoutException ex)
        {
            //  azureLogService.Debug("Analyze Exception", ex.ToString());
            log.LogError(ex.ToString());
            var version = typeof(AnalyzeController).Assembly.GetName().Version;
            faultLogger.Add(DateTime.Now, version.ToString(), input, log);
        }
    }

}