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
    public class ActivityTraceLogger :  TraceLogger, IActivityLogger
    {
        public ActivityTraceLogger(string traceSourceName, int eventId) : base(traceSourceName, eventId)
        {
        }

        public void Add(ActivityEntity entity)
        {
            ts.TraceData(TraceEventType.Information, id, entity);
        }        
    }
}
