// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading;

namespace ShaderTools.CodeAnalysis.Log
{
    internal static class Logger
    {
        private static ILogger s_currentLogger;

        /// <summary>
        /// give a way to explicitly set/replace the logger
        /// </summary>
        public static ILogger SetLogger(ILogger logger)
        {
            // we don't care what was there already, just replace it explicitly
            return Interlocked.Exchange(ref s_currentLogger, logger);
        }

        /// <summary>
        /// ensure we have a logger by putting one from workspace service if one is not there already.
        /// </summary>
        public static ILogger GetLogger()
        {
            return s_currentLogger;
        }

        public static void Log(string message)
        {
            if (string.IsNullOrEmpty(message))
                return;

            s_currentLogger?.Log(message);
        }

        public static void Log(Exception ex)
        {
            if (ex != null)
                Log(ex.ToString());
        }
    }
}
