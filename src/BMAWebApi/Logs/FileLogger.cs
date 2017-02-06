// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace BMAWebApi.Logs
{
    public abstract class FileLogger<T>
    {
        private readonly Mutex mutex;
        private readonly StreamWriter logWriter;
        private readonly DirectoryInfo dir;


        public FileLogger(Mutex mutex, DirectoryInfo dir, string filenamePattern)
        {
            if (mutex == null) throw new ArgumentNullException("mutex");
            this.mutex = mutex;
            this.dir = dir;
            dir.Create();
            logWriter = OpenFile(mutex, dir, filenamePattern, WriteHeader);
        }
        public void Close()
        {
            logWriter.Close();
        }

        public void Append(T t)
        {
            if (!mutex.WaitOne(10000))
            {
                Trace.WriteLine("Failed to lock log file; the log entry will not be in the log");
                return;
            }
            try
            {
                logWriter.BaseStream.Seek(0, SeekOrigin.End);
                WriteLine(logWriter, t);
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Failed to write a line to the log file: " + ex);
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }


        protected abstract void WriteHeader(StreamWriter w);

        protected abstract void WriteLine(StreamWriter w, T t);

        private static StreamWriter OpenFile(Mutex mutex, DirectoryInfo dir, string fileNamePattern, Action<StreamWriter> writeHeader)
        {
            for (int attempt = 100; --attempt >= 0;)
            {
                if (!mutex.WaitOne(1000)) continue;
                try
                {
                    FileInfo fileInfo = dir.GetFiles(string.Format(fileNamePattern, "*"), SearchOption.TopDirectoryOnly).OrderBy(fi => fi.Name).LastOrDefault();
                    StreamWriter w = null;
                    if (fileInfo == null || fileInfo.Length > 100 * 1048576) // no file or it is too big already (>= 100 MB)
                    {
                        string pattern = string.Format(fileNamePattern, DateTime.UtcNow.ToString("o").Replace(':', '-')); // yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffffffzz
                        string path = Path.Combine(dir.FullName, pattern);

                        var s = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.ReadWrite, 4096, FileOptions.WriteThrough);
                        w = new StreamWriter(s);
                        w.AutoFlush = true;
                        writeHeader(w);
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
                    Trace.WriteLine("Failed to create a log file: " + ioExc);
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
                Thread.Sleep(10);
            }
            throw new InvalidOperationException("Failed to create a log file");
        }



        public static DirectoryInfo ResolveDirectory(string dir, bool tryServerPath)
        {
            string basePath;
            if (tryServerPath && HttpContext.Current != null) // if runs as a part of Web Application
                basePath = HttpContext.Current.Server.MapPath(@"~\" + dir);
            else
                basePath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), dir);
            return new DirectoryInfo(basePath);
        }
    }
}
