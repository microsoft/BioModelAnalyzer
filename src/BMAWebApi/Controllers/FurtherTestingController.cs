// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
using BMAWebApi;
using System.Net.Http;
using System.Threading.Tasks;

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
