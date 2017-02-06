// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMAWebApi.Logs
{
    public class TraceLogger 
    {
        protected readonly TraceSource ts;
        protected readonly int id;

        public TraceLogger(string traceSourceName, int id)
        {
            if (String.IsNullOrEmpty(traceSourceName)) throw new ArgumentNullException("traceSourceName");

            this.id = id;

            ts = new TraceSource(traceSourceName);
        }
    }
}
