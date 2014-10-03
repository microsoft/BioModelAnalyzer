using System;
using System.Collections.Generic;
using System.Diagnostics;
using BioCheckAnalyzerCommon;

namespace BioCheck.Web.Analysis
{
    public class DefaultLogService : ILogService
    {
        private readonly List<string> debugMessages;
        private readonly List<string> errorMessages;

        public DefaultLogService()
        {
            this.debugMessages = new List<string>();
            this.errorMessages = new List<string>();
        }

        /// <summary>
        /// Gets the value of the <see cref="DebugMessages"/> property.
        /// </summary>
        public List<string> DebugMessages
        {
            get { return debugMessages; }
        }

        /// <summary>
        /// Gets the value of the <see cref="ErrorMessages"/> property.
        /// </summary>
        public List<string> ErrorMessages
        {
            get { return errorMessages; }
        }

        public void LogDebug(string message)
        {
            string timedMessage = string.Format("{0}: {1}", DateTime.Now.ToString("HH:mm:ss.fff"), message);
            this.debugMessages.Add(timedMessage);
            Debug.WriteLine(timedMessage);
        }

        public void LogError(string message)
        {
            string timedMessage = string.Format("{0}: {1}", DateTime.Now.ToString("HH:mm:ss.fff"), message);
            this.errorMessages.Add(timedMessage);
            Debug.WriteLine("ERROR: " + timedMessage);
        }
    }
}