using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ShaderTools.LanguageServer
{
    internal static class Program
    {
        public static async Task Main(string[] args)
        {
            // TODO: Make this an option.
            var logFilePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "ShaderTools");

            if (args.Contains("--launch-debugger"))
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
                    LogLevel.Warning);
            }
            catch (Exception ex)
            {
                languageServerHost?.Dispose();
                await Console.Error.WriteLineAsync(ex.ToString());
                return;
            }

            try
            {
                await languageServerHost.WaitForExit;
            }
            catch (Exception ex)
            {
                await Console.Error.WriteLineAsync(ex.ToString());
                return;
            }
            finally
            {
                languageServerHost.Dispose();
            }
        }
    }
}
