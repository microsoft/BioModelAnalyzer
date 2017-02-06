// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
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

    public class ProgramsController : ApiController
    {
        public ProgramsOutput Post(ProgramsInput input)
        {
            var p = input.Pgm;

            var b = BackEndClassLibrary1.Class1.getPrograms(p);

            var o = new ProgramsOutput { Output = b };

            return o;
        }
    }

}
