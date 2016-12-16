using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JobsRunner
{
    public class JobResult
    {
        private string content;
        private string[] errors;

        public JobResult(string content, string[] errors)
        {
            if (content == null) throw new ArgumentNullException("content");
            if (errors == null) throw new ArgumentNullException("errors");
            this.content = content;
            this.errors = errors;
        }

        public string Content { get { return content; } }

        public string[] Errors { get { return errors; } }
    }

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
        public static JobResult RunToCompletion(string executableName, string inputData, int timeoutMs)
        {
            string outputFile = Path.GetTempFileName();
            string inputFile = Path.GetTempFileName();
            string errorsFile = Path.GetTempFileName();
            File.WriteAllText(inputFile, inputData);
            try
            {
                if (!Path.IsPathRooted(executableName))
                    executableName = Path.Combine(Environment.CurrentDirectory, executableName);

                using (Process p = new Process())
                {
                    p.StartInfo.FileName = executableName;
                    p.StartInfo.Arguments = String.Format("\"{0}\" \"{1}\" \"{2}\"", inputFile, outputFile, errorsFile);
                    p.StartInfo.CreateNoWindow = true;
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.RedirectStandardError = true;
                    p.StartInfo.RedirectStandardOutput = true;

                    StringBuilder errors = new StringBuilder();
                    StringBuilder output = new StringBuilder();
                    p.ErrorDataReceived += (sender, args) =>
                    {
                        errors.AppendLine(args.Data);
                        Trace.WriteLine(args.Data, "ERROR");
                    };
                    p.OutputDataReceived += (sender, args) =>
                    {
                        output.AppendLine(args.Data);
                        Trace.WriteLine(args.Data, "OUTPUT");
                    };

                    Trace.WriteLine(String.Format("Starting process '{0} {1}'", p.StartInfo.FileName, p.StartInfo.Arguments));

                    try
                    {
                        p.Start();

                        if (!p.WaitForExit(timeoutMs)) // timeout
                        {
                            Trace.WriteLine("The process will be killed because it has been executing for too long");
                            p.Kill();
                            throw new System.TimeoutException("Allowed process execution time was exceeded");
                        }
                    }
                    catch(ThreadAbortException)
                    {
                        Trace.WriteLine("The thread is requested to abort; killing the process...");
                        p.Kill();
                    }

                    p.WaitForExit(); // To ensure that the async events are completed
                    Trace.WriteLine(String.Format("The process has exited with code {0}", p.ExitCode));
                    if(p.ExitCode == 0)
                    {
                        string outputData = File.ReadAllText(outputFile);
                        string[] pErrors;
                        if (File.Exists(errorsFile))
                            pErrors = File.ReadAllLines(errorsFile);
                        else
                            pErrors = new string[0];

                        return new JobResult(outputData, pErrors);
                    }
                    throw new InvalidOperationException(String.Format("The process has exited with code {0}; errors: {1}; output: {2}", p.ExitCode, errors.ToString(), output.ToString()));
                }
            }
            finally
            {
                File.Delete(inputFile);
                File.Delete(outputFile);
                File.Delete(errorsFile);
            }
        }
    }
}
