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