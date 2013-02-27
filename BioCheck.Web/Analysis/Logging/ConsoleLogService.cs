using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using BioCheckAnalyzerCommon;

namespace BioCheck.Web.Analysis
{
    public class ConsoleLogService : ILogService
    {
        public void LogDebug(string message)
        {
            string timedMessage = DateTime.Now.ToString("HH:mm:ss.fff") + ": " + message;
            Debug.WriteLine(timedMessage);
        }

        public void LogError(string message)
        {
            string timedMessage = DateTime.Now.ToString("HH:mm:ss.fff") + ": " + message;
            Debug.WriteLine("ERROR: " + timedMessage);
        }
    }
}