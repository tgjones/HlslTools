using System;
using CommandLine;
using ShaderTools.EditorServices.Protocol.Hlsl.Server;
using ShaderTools.EditorServices.Protocol.MessageProtocol.Channel;
using ShaderTools.EditorServices.Protocol.Server;

namespace ShaderTools.EditorServices.Host
{
    internal static class Program
    {
        public static int Main(string[] args)
        {
            return Parser.Default.ParseArguments<ProgramOptions>(args)
                .MapResult(options =>
                {
                    EditorServicesHost editorServicesHost = null;
                    try
                    {
                        editorServicesHost = new EditorServicesHost(
                            x => CreateLanguageServer(options.Language, x), 
                            options.WaitForDebugger);

                        editorServicesHost.StartLogging(options.LogFilePath, options.LogLevel);
                        editorServicesHost.StartLanguageService(options.LanguageServerPort);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("ShaderTools Editor Services host initialization failed, terminating.");
                        Console.WriteLine(ex);
                        return 2;
                    }

                    try
                    {
                        editorServicesHost.WaitForCompletion();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Caught error while waiting for Editor Services host to complete.");
                        Console.WriteLine(ex);
                        return 3;
                    }

                    return 0;
                },
                errors => 1);
        }

        private static LanguageServerBase CreateLanguageServer(HostLanguage language, ChannelBase channel)
        {
            switch (language)
            {
                case HostLanguage.Hlsl:
                    return new HlslLanguageServer(channel);

                default:
                    throw new ArgumentOutOfRangeException(nameof(language));
            }
        }
    }
}
