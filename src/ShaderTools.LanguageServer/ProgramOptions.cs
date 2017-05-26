using CommandLine;
using ShaderTools.LanguageServer.Protocol.Utilities;

namespace ShaderTools.LanguageServer
{
    internal sealed class ProgramOptions
    {
        [Option(HelpText = "Set whether to wait for the debugger or not")]
        public bool WaitForDebugger { get; set; }

        [Option(HelpText = "Fully qualified path to the log file")]
        public string LogFilePath { get; set; }

        [Option]
        public LogLevel LogLevel { get; set; }
    }
}
