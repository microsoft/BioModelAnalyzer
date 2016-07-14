using BioCheckAnalyzerCommon;
using bma.client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Http;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Linq;
using Microsoft.FSharp.Core;
using BioModelAnalyzer;
using BMAWebApi;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.ServiceRuntime;
using System.Threading.Tasks;
using System.Net.Http;

namespace bma.client.Controllers
{
    public class FurtherTestingController : JobController
    {
        public FurtherTestingController(IFailureLogger logger) : base("FurtherTesting.exe", logger)
        {
        }

        public Task<HttpResponseMessage> Post()
        {
            return ExecuteAsync((int)Utilities.GetTimeLimitFromConfig().TotalMilliseconds);
        }
    }
}