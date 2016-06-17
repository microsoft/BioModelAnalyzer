using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using bma.Cloud;
using System.IO;
using Newtonsoft.Json;
using bma.LTLPolarity;

namespace LTLCheckRole
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);

        private IWorker worker;

        public override void Run()
        {
            Trace.TraceInformation("LTLCheckRole is running");

            try
            {
                worker.Process(DoJob, TimeSpan.FromSeconds(1.0), TimeSpan.FromMinutes(2.0));
            }
            finally
            {
                this.runCompleteEvent.Set();
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            bool result = base.OnStart();

            var storageAccount = CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue("StorageConnectionString"));
            var schedulerName = "ltlpolarity"; // todo: can differ for different controllers; use setter injection with name?

            try
            {
                if (result)
                {
                    var settings = new WorkerSettings(TimeSpan.FromHours(1), 3, TimeSpan.FromMinutes(5));
                    worker = Worker.Create(storageAccount, schedulerName, settings);
                    Trace.TraceInformation("LTLCheckRole has been started");
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Exception during OnStart: {0}", ex);
                result = false;
            }

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("LTLCheckRole is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            if (worker != null)
                worker.Dispose();

            Trace.TraceInformation("LTLCheckRole has stopped");
        }
        

        private Stream DoJob(Guid jobId, Stream input)
        {
            Trace.TraceInformation("Doing the job {0}", jobId);
            var sw = new Stopwatch();
            sw.Start();

            var reader = new StreamReader(input);
            var input_s = reader.ReadToEnd();
            var query = JsonConvert.DeserializeObject<LTLPolarityAnalysisInputDTO>(input_s);

            var res = bma.LTLPolarity.Algorithms.Check(query);

            var jsRes = JsonConvert.SerializeObject(res);
            var output_s = jsRes.ToString();
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write(output_s);
            writer.Flush();
            ms.Position = 0;

            sw.Stop();
            Trace.TraceInformation("Job {0} done ({1})", jobId, sw.Elapsed);

            return ms;
        }
    }
}
