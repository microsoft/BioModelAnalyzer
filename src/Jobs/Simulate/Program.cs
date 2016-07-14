using bma.BioCheck;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulate
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2) throw new System.ArgumentException("Incorrect number of arguments");
            var inputJson = File.ReadAllText(args[0]);
            var query = JsonConvert.DeserializeObject<SimulationInput>(inputJson);

            var res = Simulation.Simulate(query);

            var jsRes = JsonConvert.SerializeObject(res);
            File.WriteAllText(args[1], jsRes);
        }
    }
}
