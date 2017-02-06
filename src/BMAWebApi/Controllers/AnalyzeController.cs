// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
using bma.BioCheck;
using BMAWebApi;
using System;
using System.Web.Http;

namespace bma.client.Controllers
{
    public class AnalyzeController : ApiController
    {
        private readonly IFailureLogger faultLogger;

        public AnalyzeController(IFailureLogger logger)
        {
            if (logger == null) throw new ArgumentNullException("logger");
            this.faultLogger = logger;
        }

        public AnalysisOutput Post([FromBody]AnalysisInput input)
        {
            var log = new DefaultLogService();
            var output = Utilities.RunWithTimeLimit(() => Analysis.Analyze(input), Utilities.GetTimeLimitFromConfig());
            if (output.ErrorMessages != null && output.ErrorMessages.Length > 0)
            {
                var contents = new LogContents(output.DebugMessages, output.ErrorMessages);
                faultLogger.Add(DateTime.Now, typeof(JobController).Assembly.GetName().Version.ToString(), input, contents);
            }
            return output;
        }
    }
}
