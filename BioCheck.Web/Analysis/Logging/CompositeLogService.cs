using System.Collections.Generic;
using BioCheckAnalyzerCommon;

namespace BioCheck.Web.Analysis
{
    public class CompositeLogService : ILogService
    {
        private readonly List<ILogService> loggers;

        public CompositeLogService(IEnumerable<ILogService> logs)
        {
            this.loggers = new List<ILogService>(logs);
        }

        /// <summary>
        /// Gets the value of the <see cref="Loggers"/> property.
        /// </summary>
        public List<ILogService> Loggers
        {
            get { return loggers; }
        }

        public void LogDebug(string message)
        {
            this.loggers.ForEach(ls => ls.LogDebug(message));
        }

        public void LogError(string message)
        {
            this.loggers.ForEach(ls => ls.LogError(message));
        }
    }
}