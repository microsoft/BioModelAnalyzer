// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
using BMAWebApi;
using System.Net.Http;
using System.Threading.Tasks;

namespace bma.client.Controllers
{
    public class AnalyzeLTLPolarityController : JobController
    {
        public AnalyzeLTLPolarityController(IFailureLogger logger) : base("AnalyzeLTL.exe", logger)
        {
        }

        public Task<HttpResponseMessage> Post()
        {
            return ExecuteAsync(60000);
        }
    }

}
