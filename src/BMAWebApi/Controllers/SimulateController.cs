using BioCheckAnalyzerCommon;
using BioModelAnalyzer;
using bma.BioCheck;
using BMAWebApi;
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
    public class SimulateController : ApiController
    {
        private readonly IFailureLogger faultLogger;


        public SimulateController(IFailureLogger logger)
        {
            if (logger == null) throw new ArgumentNullException("logger");
            this.faultLogger = logger;
        }

        public SimulationOutput Post([FromBody]SimulationInput input)
        {
            var log = new DefaultLogService();
            var output = Utilities.RunWithTimeLimit(() => Simulation.Simulate(input), Utilities.GetTimeLimitFromConfig());
            if (output.ErrorMessages != null && output.ErrorMessages.Length > 0)
            {
                var contents = new LogContents(output.DebugMessages, output.ErrorMessages);
                faultLogger.Add(DateTime.Now, typeof(JobController).Assembly.GetName().Version.ToString(), input, contents);
            }
            return output;
        }
    }
}