using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

//using BackendW32Project;

namespace WebApi
{
    public class SimulationController : ApiController
    {
        public SimulationOutput Post(SimulationInput input)
        {
            var p = input.Pgm;
            var c = input.Condition;

            var b = BackEndClassLibrary1.Class1.foo(p);

            var o = new SimulationOutput { Output = p + c + b};
            
            return o;
        }
    }
}
