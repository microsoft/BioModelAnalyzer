// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
using bma.LTL;
using Newtonsoft.Json;
using System.IO;

namespace SimulateLTL
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2 || args.Length > 3) throw new System.ArgumentException("Incorrect number of arguments");
            var inputJson = File.ReadAllText(args[0]);
            var query = JsonConvert.DeserializeObject<LTLSimulationAnalysisInputDTO>(inputJson);

            var res = Analysis.Simulate(query);

            var jsRes = JsonConvert.SerializeObject(res);
            File.WriteAllText(args[1], jsRes);
            if (args.Length >= 3)
            {
                File.WriteAllLines(args[2], (res != null && res.ErrorMessages != null) ? res.ErrorMessages : new string[0]);
            }
        }
    }
}
