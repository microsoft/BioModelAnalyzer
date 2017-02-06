// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
using BMAWebApi.Logs;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;

namespace BMAWebApi
{
    public class FailureFileLogger : FileLogger<FailureDescription>, IFailureLogger
    {
        private static Mutex mutex = new Mutex(false, "BMAWebApi.FailureFileLogger");

        private readonly DirectoryInfo dataDir;

        public FailureFileLogger(DirectoryInfo dir) : base(mutex, dir, "failures_{0}.csv")
        {
            if (dir == null) throw new ArgumentNullException("dir");
            dataDir = dir.CreateSubdirectory("requests");
        }

        public FailureFileLogger(string dir, bool tryServerPath) : this(ResolveDirectory(dir, tryServerPath))
        {
        }

        protected override void WriteHeader(StreamWriter w)
        {
            w.WriteLine("Entry Id, Timestamp, Backend version");
        }

        protected override void WriteLine(StreamWriter w, FailureDescription f)
        {
            w.WriteLine(string.Join(", ", new string[] { f.UniqueName, f.DateTime.ToString("o"), f.BackEndVersion }));
        }

        public void Add(DateTime dateTime, string backEndVersion, object request, ILogContents log)
        {
            var uniqueName = Guid.NewGuid().ToString();

            string inputBlobName = uniqueName + "_request.json";
            try
            {
                string jsonRequest = JsonConvert.SerializeObject(request, Formatting.Indented);
                File.WriteAllText(Path.Combine(dataDir.FullName, inputBlobName), jsonRequest);
            }
            catch (Exception exc)
            {
                Trace.WriteLine("Error writing file with request: " + exc.Message);
                inputBlobName = null;
            }

            string outputBlobName = uniqueName + "_result.json";
            try
            {
                using (var stream = File.OpenWrite(Path.Combine(dataDir.FullName, outputBlobName)))
                using (var writer = new StreamWriter(stream))
                {

                    if (log.ErrorMessages != null && log.ErrorMessages.Length > 0)
                        writer.WriteLine("Error messages:\n{0}\n\n", String.Join("\n", log.ErrorMessages));
                    if (log.DebugMessages != null && log.DebugMessages.Length > 0)
                        writer.WriteLine("Debug messages:\n{0}\n", String.Join("\n", log.DebugMessages));
                    writer.Flush();
                }
            }
            catch (Exception exc)
            {
                Trace.WriteLine("Error writing file with result: " + exc.Message);
                outputBlobName = null;
            }

            var failure = new FailureDescription
            {
                UniqueName = uniqueName,
                BackEndVersion = backEndVersion,
                DateTime = dateTime
            };
            Append(failure);
        }
    }


    public struct FailureDescription
    {
        public string UniqueName { get; set; }
        public string BackEndVersion { get; set; }
        public DateTime DateTime { get; set; }
    }
}
