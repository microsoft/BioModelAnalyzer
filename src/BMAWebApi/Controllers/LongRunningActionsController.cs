using bma.Cloud;
using BMAWebApi;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;


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
        public HttpResponseMessage Get(Guid appId, Guid jobId)
        {
            var status = scheduler.TryGetStatus(appId, jobId);
            if (status != null)
            {
                var st = status.Value;
                switch (st.Tag)
                {
                    case JobStatusWithInfo.Tags.Succeeded:
                        return HttpResponses.PlainText(Request, "Succeeded", HttpStatusCode.OK /* 200 */);
                    case JobStatusWithInfo.Tags.Queued:
                        return HttpResponses.PlainText(Request, ((JobStatusWithInfo.Queued)st).position.ToString(), HttpStatusCode.Created /* 201 */);
                    case JobStatusWithInfo.Tags.Executing:
                        var x = (JobStatusWithInfo.Executing)st;
                        int elapsed = (int)(DateTimeOffset.Now.Subtract(x.started)).TotalMilliseconds;
                        JObject json = new JObject(new object[] {
                            new JProperty("started", x.started.ToString("o")),
                            new JProperty("elapsed", elapsed) });
                        return HttpResponses.Json(Request, json.ToString(), HttpStatusCode.Accepted /* 202 */);
                    case JobStatusWithInfo.Tags.Failed:
                        return HttpResponses.PlainText(Request, ((JobStatusWithInfo.Failed)st).message, (HttpStatusCode)203);
                }
                return Request.CreateResponse((HttpStatusCode)501, new HttpError("Unknown status"));
            }

            return Request.CreateResponse(HttpStatusCode.NotFound, new HttpError("Job not found"));
        }

        // DELETE /api/lra/{appId} ? jobId=GUID
        // where {appId} is the application ID.
        // Deletes the job and, if appropriate, cancels the execution.
        // Returns 404 if there is no such job or appId is incorrect.
        public HttpResponseMessage DeleteJob(Guid appId, Guid jobId)
        {
            try
            {
                if (scheduler.DeleteJob(appId, jobId))
                    return Request.CreateResponse(HttpStatusCode.OK);
                return Request.CreateResponse(HttpStatusCode.NotFound, new HttpError("Job not found"));
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new HttpError(ex, false));
            }
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
        public HttpResponseMessage GetResult(Guid appId, Guid jobId)
        {
            var result = scheduler.TryGetResult(appId, jobId);
            if (result != null)
            {
                using (StreamReader reader = new StreamReader(result.Value))
                {
                    var s = reader.ReadToEnd();
                    return HttpResponses.Json(Request, s);
                }
            }
            return Request.CreateResponse(HttpStatusCode.NotFound, new HttpError("Job result is not available"));
        }
    }
}