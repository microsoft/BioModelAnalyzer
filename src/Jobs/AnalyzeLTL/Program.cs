using bma.LTL;
using Newtonsoft.Json;
using System.IO;

namespace AnalyzeLTL
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2 || args.Length > 3) throw new System.ArgumentException("Incorrect number of arguments");
            var inputJson = File.ReadAllText(args[0]);
            var query = JsonConvert.DeserializeObject<LTLPolarityAnalysisInputDTO>(inputJson);

            var res = Analysis.Polarity(query);

            var jsRes = JsonConvert.SerializeObject(res);
            File.WriteAllText(args[1], jsRes);

            if(args.Length >= 3)
            {
                using (StreamWriter w = new StreamWriter(File.OpenWrite(args[2])))
                {
                    if (res.Item1 != null && res.Item1.ErrorMessages != null) {
                        foreach(string error in res.Item1.ErrorMessages)
                            w.WriteLine(error);
                    }
                    if (res.Item2 != null && res.Item2.ErrorMessages != null)
                    {
                        foreach (string error in res.Item2.ErrorMessages)
                            w.WriteLine(error);
                    }
                    w.Close();
                }
            }
        }
    }
}
