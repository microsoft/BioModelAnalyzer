// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
using bma.Cloud;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;

namespace LTLCheckRole
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);
        
        private IWorker worker;
        private string basePath;

        public override void Run()
        {
            Trace.TraceInformation("LTLCheckRole is running");

            try
            {
                basePath = Environment.CurrentDirectory;
                worker.Process(DoJob, TimeSpan.FromSeconds(0.5), TimeSpan.FromMinutes(1.0));
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
                    var settings = new WorkerSettings(TimeSpan.FromHours(1), 3, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(1));
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

            string input_s = (new StreamReader(input)).ReadToEnd();

            // JobRunner kills the process, if this thread is aborted due to job cancellation.
            JobsRunner.JobResult result = JobsRunner.Job.RunToCompletion(Path.Combine(basePath, "AnalyzeLTL.exe"), input_s, -1);

            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write(result.Content);
            writer.Flush();
            ms.Position = 0;

            sw.Stop();
            Trace.TraceInformation("Job {0} done ({1})", jobId, sw.Elapsed);

            return ms;
        }
    }
}
