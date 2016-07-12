using BioCheckAnalyzerCommon;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace bma.LTLPolarity
{
    public class LogService : ILogService
    {
        private readonly List<string> debugMessages;
        private readonly List<string> errorMessages;

        public LogService()
        {
            this.debugMessages = new List<string>();
            this.errorMessages = new List<string>();
        }

        public string[] DebugMessages
        {
            get { return debugMessages.ToArray(); }
        }

        public string[] ErrorMessages
        {
            get { return errorMessages.ToArray(); }
        }

        public void LogDebug(string message)
        {
            string timedMessage = string.Format("{0}: {1}", DateTime.Now.ToString("HH:mm:ss.fff"), message);
            this.debugMessages.Add(timedMessage);
            Trace.WriteLine(timedMessage);
        }

        public void LogError(string message)
        {
            string timedMessage = string.Format("{0}: {1}", DateTime.Now.ToString("HH:mm:ss.fff"), message);
            this.errorMessages.Add(timedMessage);
            Trace.WriteLine("ERROR: " + timedMessage);
        }
    }
}
