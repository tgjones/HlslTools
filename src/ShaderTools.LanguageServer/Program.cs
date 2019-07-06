using System;
using System.CommandLine;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ShaderTools.LanguageServer
{
    internal static class Program
    {
        public static async Task Main(string[] args)
        {
            var launchDebugger = false;
            string logFilePath = null;
            var logLevel = LogLevel.Warning;

            logFilePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "ShaderTools");

            ArgumentSyntax.Parse(args, syntax =>
            {
                syntax.DefineOption("launchdebugger", ref launchDebugger, false, "Set whether to launch the debugger or not.");
                //syntax.DefineOption("logfilepath", ref logFilePath, true, "Fully qualified path to the log file.");
                syntax.DefineOption("loglevel", ref logLevel, x =>  (LogLevel) Enum.Parse(typeof(LogLevel), x), false, "Logging level.");
            });

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
