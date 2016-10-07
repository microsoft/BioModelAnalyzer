using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApi
{
    public class SimulationInput
    {
        public string Pgm { get; set; }
        public string Condition { get; set; }
    }

    public class SimulationOutput
    { 
        public string Output { get; set; } 
    }

    public class ProgramsOutput
    {
        public string Output { get; set; }
    }
    public class ProgramsInput
    {
        public string Pgm { get; set; }
    }

    public class ProgramsCellInput
    {
        public string Pgm { get; set; }
        public string Cell { get; set; }
    }

    public class ProgramsCellCond
    {
        public string Pgm { get; set; }
        public string Cell { get; set;  }
        public string Cond { get; set;  }
    }

}
