using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobsRunner
{
    public static class Job
    {
        /// <summary>
        /// Runs the given executable passing given input data as a file and returns content of the output file, produced by the executable.
        /// </summary>
        /// <param name="executableName">Either relative or absolute path to the executable file. 
        /// If it is relative, the current directory is the base path.
        /// The executable must accept two arguments; one is the path of an input file,
        /// second is the path of an output file.</param>
        /// <param name="inputData">A text which will be saved to a file and passed as the first argument.</param>
        /// <param name="timeoutMs">Maximum allowed execution time (milliseconds); if the executable doesn't end for that time, it is killed,
        /// and the method throws TimeoutException.</param>        
        /// <returns>Content of the file produced by the executable, if the exit code is 0. Otherwise, throws an exception.</returns>
        /// <remarks>
        /// Standard error and output streams of the executable file will be redirected to the System.Diagnostics.Trace.
        /// </remarks>
        public static string RunToCompletion(string executableName, string inputData, int timeoutMs)
        {
            string outputFile = Path.GetTempFileName();
            string inputFile = Path.GetTempFileName();
            File.WriteAllText(inputFile, inputData);
            try
            {
                if (!Path.IsPathRooted(executableName))
                    executableName = Path.Combine(Environment.CurrentDirectory, executableName);

                using (Process p = new Process())
                {
                    p.StartInfo.FileName = executableName;
                    p.StartInfo.Arguments = String.Format("\"{0}\" \"{1}\"", inputFile, outputFile);
                    p.StartInfo.CreateNoWindow = true;
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.RedirectStandardError = true;
                    p.StartInfo.RedirectStandardOutput = true;

                    StringBuilder errors = new StringBuilder();
                    p.ErrorDataReceived += (sender, args) =>
                    {
                        errors.AppendLine(args.Data);
                        Trace.WriteLine(args.Data, "ERROR");
                    };
                    p.OutputDataReceived += (sender, args) =>
                    {
                        Trace.WriteLine(args.Data, "OUTPUT");
                    };

                    Trace.WriteLine(String.Format("Starting process '{0} {1}'", p.StartInfo.FileName, p.StartInfo.Arguments));
                    p.Start();

                    if (!p.WaitForExit(timeoutMs)) // timeout
                    {
                        Trace.WriteLine("The process will be killed because it has been executing for too long");
                        p.Kill();
                        throw new TimeoutException("Allowed process execution time was exceeded");
                    }
                    p.WaitForExit(); // To ensure that the async events are completed
                    Trace.WriteLine(String.Format("The process has exited with code {0}", p.ExitCode));
                    if(p.ExitCode == 0)
                    {
                        string outputData = File.ReadAllText(outputFile);
                        return outputData;
                    }
                    throw new InvalidOperationException(String.Format("The process has exited with code {0}; errors: {1}", p.ExitCode, errors.ToString()));
                }
            }
            finally
            {
                File.Delete(inputFile);
                File.Delete(outputFile);
            }
        }
    }
}
