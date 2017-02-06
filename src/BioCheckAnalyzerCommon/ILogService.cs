// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioCheckAnalyzerCommon
{
    /// <summary>
    /// Interface to a logging service for the Analyzers.
    /// </summary>
    public interface ILogService
    {
        /// <summary>
        /// Log a debug information message.
        /// </summary>
        /// <param name="message">The message.</param>
        void LogDebug(string message);

        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="message">The message.</param>
        void LogError(string message);
    }
}
