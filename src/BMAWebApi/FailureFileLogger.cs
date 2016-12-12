using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;

namespace BMAWebApi
{
    public class FailureFileLogger : IFailureLogger
    {
        private static Mutex mutex = new Mutex(false, "BMAWebApi.FailureFileLogger");

        private static DirectoryInfo ResolveDirectory(string dir, bool tryServerPath)
        {
            string basePath;
            if (tryServerPath && HttpContext.Current != null) // if runs as a part of Web Application
                basePath = HttpContext.Current.Server.MapPath(@"~\" + dir);
            else
                basePath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), dir);
            return new DirectoryInfo(basePath);
        }

        private readonly StreamWriter failuresFile;
        private readonly DirectoryInfo dir;
        private readonly DirectoryInfo dataDir;

        public FailureFileLogger(DirectoryInfo dir)
        {
            if (dir == null) throw new ArgumentNullException("dir");
            this.dir = dir;
            dir.Create();
            dataDir = dir.CreateSubdirectory("requests");
            failuresFile = PrepareLogFile(dir);
        }

        public FailureFileLogger(string dir, bool tryServerPath) : this(ResolveDirectory(dir, tryServerPath))
        {
        }

        private static StreamWriter PrepareLogFile(DirectoryInfo dir)
        {
            for (int attempt = 100; --attempt >= 0;)
            {
                if (!mutex.WaitOne(1000)) continue;
                try
                {
                    FileInfo fileInfo = dir.GetFiles("failures_*.csv", SearchOption.TopDirectoryOnly).OrderBy(fi => fi.Name).LastOrDefault();
                    StreamWriter w = null;
                    if (fileInfo == null || fileInfo.Length > 1048576) // no file or it is too big already
                    {
                        string pattern = string.Format("failures_{0}.csv", DateTime.UtcNow.ToString("o").Replace(':', '-')); // yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffffffzz
                        string path = Path.Combine(dir.FullName, pattern);

                        var s = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.ReadWrite, 4096, FileOptions.WriteThrough);
                        w = new StreamWriter(s);
                        w.AutoFlush = true;
                        w.WriteLine("Entry Id, Timestamp, Backend version");
                    }
                    else
                    {
                        var s = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Write, FileShare.ReadWrite, 4096, FileOptions.WriteThrough);
                        w = new StreamWriter(s);
                        w.AutoFlush = true;
                    }                    
                    
                    return w;
                }
                catch (IOException ioExc)
                {
                    Trace.WriteLine("Failed to create failures log file: " + ioExc);
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
                Thread.Sleep(10);
            }
            throw new InvalidOperationException("Failed to create a file to log failures");
        }

        private void AppendLineSafe(Failure f)
        {
            if (!mutex.WaitOne(10000))
            {
                Trace.WriteLine("Failed to lock log file; the log entry will not be in the log");
                return;
            }
            try 
            {
                failuresFile.BaseStream.Seek(0, SeekOrigin.End);
                failuresFile.WriteLine(string.Format("{0}, {1}, {2}", f.UniqueName, f.DateTime.ToString("o"), f.BackEndVersion));
            }
            catch(Exception ex)
            {
                Trace.WriteLine("Failed to write a line to the log file: " + ex);
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }

        public void Close()
        {
            failuresFile.Close();
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

            var failure = new Failure
            {
                UniqueName = uniqueName,
                BackEndVersion = backEndVersion,
                DateTime = dateTime
            };
            AppendLineSafe(failure);
        }

        internal struct Failure
        {
            public string UniqueName { get; set; }
            public string BackEndVersion { get; set; }
            public DateTime DateTime { get; set; }
        }
    }
}
