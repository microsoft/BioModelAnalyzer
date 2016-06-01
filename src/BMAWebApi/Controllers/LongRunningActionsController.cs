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

        public string Get()
        {
            return "HELLO!";
        }
                
        public async Task<Guid> PostJob()
        {
            var request = await Request.Content.ReadAsStreamAsync();
            var appId = Guid.NewGuid(); // todo: take from the request

            Job job = new Job(appId, request);
            var jobId = scheduler.AddJob(job);

            return jobId;
        }
    }

}