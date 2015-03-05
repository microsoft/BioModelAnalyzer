using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApi
{
    public class SimulationInput
    {
        public string[] Pgm { get; set; }
        public string Condition { get; set; }
    }

    public class SimulationOutput
    { 
        public string Output { get; set; } 
    }
}
