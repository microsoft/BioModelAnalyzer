using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace WebApi
{
    public class SimulationController : ApiController
    {
        public SimulationOutput Post(SimulationInput input)
        {
            var o = new SimulationOutput { Output = "SimOut" };
            return o;
        }
    }
}
