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
    public class ActivityFileLogger :  FileLogger<ActivityEntity>, IActivityLogger
    {
        private static Mutex mutex = new Mutex(false, "BMAWebApi.ActivityFileLogger");

        public ActivityFileLogger(DirectoryInfo dir) : base(mutex, dir, "activity_{0}.csv")
        {
        }

        public ActivityFileLogger(string dir, bool tryServerPath) : this(ResolveDirectory(dir, tryServerPath))
        {
        }

        public void Add(ActivityEntity entity)
        {
            Append(entity);
        }

        protected override void WriteHeader(StreamWriter w)
        {
            w.WriteLine("SessionID, UserID, LogInTime, LogOutTime, RunProofCount, RunSimulationCount, NewModelCount, ImportModelCount, SaveModelCount, FurtherTestingCount, ClientVersion, SimulationErrorCount, FurtherTestingErrorCount, ProofErrorCount, AnalyzeLTLCount, AnalyzeLTLErrorCount");
        }

        protected override void WriteLine(StreamWriter w, ActivityEntity t)
        {
            string line = string.Join(", ", new object[]
            {
                t.SessionID,
                t.UserID,
                t.LogInTime,
                t.LogOutTime,
                t.RunProofCount,
                t.RunSimulationCount,
                t.NewModelCount,
                t.ImportModelCount,
                t.SaveModelCount,
                t.FurtherTestingCount,
                t.ClientVersion,
                t.SimulationErrorCount,
                t.FurtherTestingErrorCount,
                t.ProofErrorCount,
                t.AnalyzeLTLCount,
                t.AnalyzeLTLErrorCount
            });
            w.WriteLine(line);
        }
        
    }
}
