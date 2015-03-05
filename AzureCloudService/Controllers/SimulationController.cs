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
            var p = input.Pgm;
            var c = input.Condition;
            var o = new SimulationOutput { Output = p + c };
            return o;
        }
    }
}
