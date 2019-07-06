using System;
using System.CommandLine;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ShaderTools.LanguageServer
{
    internal static class Program
    {
        /// <summary>
        /// ShaderTools Language Server
        /// </summary>
        /// <param name="launchDebugger">Set whether to launch the debugger or not.</param>
        /// <param name="logLevel">Logging level.</param>
        /// <returns></returns>
        public static async Task Main(bool launchDebugger = false, LogLevel logLevel = LogLevel.Warning)
        {
            // TODO: Make this an option.
            var logFilePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "ShaderTools");

            if (launchDebugger)
            {
                Debugger.Launch();
            }

            LanguageServerHost languageServerHost = null;
            try
            {
                languageServerHost = await LanguageServerHost.Create(
                    Console.OpenStandardInput(),
                    Console.OpenStandardOutput(),
                    logFilePath,
                    logLevel);
            }
            catch (Exception ex)
            {
                languageServerHost?.Dispose();
                Console.Error.WriteLine(ex);
                return;
            }

            try
            {
                await languageServerHost.WaitForExit;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return;
            }
            finally
            {
                languageServerHost.Dispose();
            }
        }
    }
}
