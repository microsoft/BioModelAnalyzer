using BioCheckAnalyzerCommon;
using BioModelAnalyzer;
using bma.Cloud;
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
    public class LongRunningActionsController : ApiController
    {
        private readonly IFailureLogger faultLogger;
        private readonly IScheduler scheduler;

        public LongRunningActionsController(IScheduler scheduler, IFailureLogger logger)
        {
            if (logger == null) throw new ArgumentNullException("logger");
            if (scheduler == null) throw new ArgumentNullException("scheduler");

            this.faultLogger = logger;
            this.scheduler = scheduler;
        }

        // GET /api/lra/{appId} ? jobId=GUID
        // where {appId} is the application ID.
        // Returns the status of the job.
        // Returns 404 if there is no such job or appId is incorrect.
        public string Get(Guid appId, Guid jobId)
        {
            var status = scheduler.TryGetStatus(appId, jobId);
            if (status != null) return bma.Cloud.Jobs.status(status.Value);

            throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = new StringContent("Job not found")
            });
        }

        // POST /api/lra/{appId},
        // where {appId} is the application ID.
        // Adds new job from the application.
        // Returns the job ID.
        public async Task<Guid> PostJob(Guid appId)
        {
            var request = await Request.Content.ReadAsStreamAsync();

            Job job = new Job(appId, request);
            var jobId = scheduler.AddJob(job);

            return jobId;
        }
    }

    public class LongRunningActionsSpecificController : ApiController
    {
        private readonly IFailureLogger faultLogger;
        private readonly IScheduler scheduler;

        public LongRunningActionsSpecificController(IScheduler scheduler, IFailureLogger logger)
        {
            if (logger == null) throw new ArgumentNullException("logger");
            if (scheduler == null) throw new ArgumentNullException("scheduler");

            this.faultLogger = logger;
            this.scheduler = scheduler;
        }

        [HttpGet]
        [ActionName("Result")]
        // GET /api/lra/{appId}/result ? jobId=GUID
        // where {appId} is the application ID.
        // Returns the status of the job.
        // Returns 404 if there is no such job or appId is incorrect.
        public string GetResult(Guid appId, Guid jobId)
        {
            var result = scheduler.TryGetResult(appId, jobId);
            if (result != null)
            {
                using(StreamReader reader = new StreamReader(result.Value))
                {
                    return reader.ReadToEnd();
                }
            }

            throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = new StringContent("Job result is not available")
            });
        }
    }
    }