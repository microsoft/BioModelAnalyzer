// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
using System;
using System.Collections.Generic;
using System.Diagnostics;
using BioCheckAnalyzerCommon;
using BMAWebApi;

namespace bma.client
{
    public class LogContents : ILogContents
    {
        private string[] debugMessages;
        private string[] errorMessages;

        public LogContents(string[] debugMessages, string[] errorMessages)
        {
            this.debugMessages = debugMessages;
            this.errorMessages = errorMessages;
        }

        public string[] DebugMessages
        {
            get
            {
                return debugMessages;
            }
        }

        public string[] ErrorMessages
        {
            get
            {
                return errorMessages;
            }
        }
    }

    public class DefaultLogService : ILogService, ILogContents
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
        public string[] DebugMessages
        {
            get { return debugMessages.ToArray(); }
        }

        /// <summary>
        /// Gets the value of the <see cref="ErrorMessages"/> property.
        /// </summary>
        public string[] ErrorMessages
        {
            get { return errorMessages.ToArray(); }
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
