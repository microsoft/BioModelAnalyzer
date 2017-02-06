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
    public class FailureTraceLogger : TraceLogger, IFailureLogger
    {
        public FailureTraceLogger(string traceSourceName, int eventId): base(traceSourceName, eventId)
        {
        }

        public void Add(DateTime dateTime, string backEndVersion, object request, ILogContents contents)
        {
            ts.TraceData(TraceEventType.Information, id, new object[] { dateTime, backEndVersion, contents });
        }
    }
}
