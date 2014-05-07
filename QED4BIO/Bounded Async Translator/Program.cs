using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Bounded_Async_Translator
{
    class Program
    {
        static void Main(string[] args)
        {
            string fileName = "mutant.smv";
            if (!System.IO.File.Exists(fileName))
            {
                System.Console.WriteLine("File can not be found");
            }

           // FileStream stream = File.Open("mutant.smv", FileMode.Open);
            
            var r = new Parser.SMV().parser_smv( fileName);
            foreach (var m in r)
            {
                System.Console.Out.WriteLine(m.name);
                
            }
        }
    }
}
