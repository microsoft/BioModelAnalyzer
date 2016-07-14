using bma.LTL;
using Newtonsoft.Json;
using System.IO;

namespace AnalyzeLTL
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2) throw new System.ArgumentException("Incorrect number of arguments");
            var inputJson = File.ReadAllText(args[0]);
            var query = JsonConvert.DeserializeObject<LTLPolarityAnalysisInputDTO>(inputJson);

            var res = Analysis.Polarity(query);

            var jsRes = JsonConvert.SerializeObject(res);
            File.WriteAllText(args[1], jsRes);
        }
    }
}
