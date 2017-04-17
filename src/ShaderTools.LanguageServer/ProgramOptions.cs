using CommandLine;
using ShaderTools.EditorServices.Utility;

namespace ShaderTools.LanguageServer
{
    internal sealed class ProgramOptions
    {
        [Option]
        public HostLanguage Language { get; set; }

        [Option(HelpText = "Port to use for the language server")]
        public int LanguageServerPort { get; set; }

        [Option(HelpText = "Set whether to wait for the debugger or not")]
        public bool WaitForDebugger { get; set; }

        [Option(HelpText = "Fully qualified path to the log file")]
        public string LogFilePath { get; set; }

        [Option]
        public LogLevel LogLevel { get; set; }
    }

    internal enum HostLanguage
    {
        Hlsl,
        ShaderLab
    }
}
