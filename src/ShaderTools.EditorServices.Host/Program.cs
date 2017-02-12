using System;
using Microsoft.Extensions.CommandLineUtils;
using ShaderTools.EditorServices.Protocol.MessageProtocol.Channel;
using ShaderTools.EditorServices.Protocol.Server;
using ShaderTools.EditorServices.Utility;

namespace ShaderTools.EditorServices.Host
{
    class Program
    {
        static void Main(string[] args)
        {
            var commandLineApplication = new CommandLineApplication(throwOnUnexpectedArg: true);

            var languageOption = commandLineApplication.Option(
                "--language <language>",
                "Language (HLSL or ShaderLab).",
                CommandOptionType.SingleValue);

            var languageServerPortOption = commandLineApplication.Option(
                "--languageServicePort <port>",
                "Port to start the language server on.",
                CommandOptionType.SingleValue);

            var waitForDebuggerOption = commandLineApplication.Option(
                "--waitForDebugger",
                "Whether to wait for the debugger.",
                CommandOptionType.NoValue);

            var logFilePathOption = commandLineApplication.Option(
                "--logFilePath <logFilePath>",
                "Path to the log file.",
                CommandOptionType.SingleValue);

            var logLevelOption = commandLineApplication.Option(
                "--logLevel <logLevel>",
                "Logging level.",
                CommandOptionType.SingleValue);

            commandLineApplication.OnExecute(() =>
            {
                HostLanguage language = HostLanguage.HLSL;
                if (!languageOption.HasValue() || !Enum.TryParse(languageOption.Value(), out language))
                {
                    Console.WriteLine("--language is required, and must be one of [HLSL, ShaderLab].");
                    return 1;
                }

                var languageServerPort = -1;
                if (!languageServerPortOption.HasValue() || !int.TryParse(languageServerPortOption.Value(), out languageServerPort))
                {
                    Console.WriteLine("--languageServerPort is required, and must be an integer.");
                    return 1;
                }

                if (!logFilePathOption.HasValue())
                {
                    Console.WriteLine("--logFilePath is required.");
                    return 1;
                }

                LogLevel logLevel = LogLevel.Normal;
                if (logLevelOption.HasValue() && !Enum.TryParse(logLevelOption.Value(), out logLevel))
                {
                    Console.WriteLine("--logLevel must be one of [Verbose, Normal, Warning, Error].");
                    return 1;
                }

                try
                {
                    var editorServicesHost = new EditorServicesHost(waitForDebuggerOption.HasValue());

                    editorServicesHost.StartLogging(logFilePathOption.Value(), logLevel);
                    editorServicesHost.StartLanguageService(GetLanguageFunc(language), languageServerPort);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("PowerShell Editor Services host initialization failed, terminating.");
                    Console.WriteLine(ex);
                    return 2;
                }

                return 0;
            });

            commandLineApplication.Execute(args);
        }

        private static Func<TcpSocketServerChannel, LanguageServerBase> GetLanguageFunc(HostLanguage language)
        {
            switch (language)
            {
                case HostLanguage.HLSL:
                    return c => new HlslLanguageServer(c);

                case HostLanguage.ShaderLab:
                    return c => new ShaderLabLanguageServer(c);

                default:
                    throw new NotSupportedException($"Language not supported: {language}");
            }
        }
    }

    public enum HostLanguage
    {
        HLSL,
        ShaderLab
    }
}
