using BMAWebApi;
using System.Net.Http;
using System.Threading.Tasks;

namespace bma.client.Controllers
{
    public class AnalyzeLTLSimulationController : JobController
    {
        public AnalyzeLTLSimulationController(IFailureLogger logger) : base("SimulateLTL.exe", logger)
        {
        }
        public Task<HttpResponseMessage> Post()
        {
            return ExecuteAsync((int)Utilities.GetTimeLimitFromConfig().TotalMilliseconds);
        }
    }

}