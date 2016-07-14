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
    public class AnalyzeLTLSimulationController : JobController
    {
        public AnalyzeLTLSimulationController(IFailureLogger logger) : base("SimulateLTL.exe", logger)
        {
        }
        public Task<HttpResponseMessage> Post()
        {
            return ExecuteAsync(60000);
        }
    }

}