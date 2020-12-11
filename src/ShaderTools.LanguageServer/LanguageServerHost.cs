using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using OmniSharp.Extensions.LanguageServer.Server;
using Serilog;
using ShaderTools.CodeAnalysis;
using ShaderTools.CodeAnalysis.Diagnostics;
using ShaderTools.CodeAnalysis.Host.Mef;
using ShaderTools.LanguageServer.Handlers;

namespace ShaderTools.LanguageServer
{
    internal sealed class LanguageServerHost : IDisposable
    {
        private ILanguageServer _server;

        private readonly MefHostServices _exportProvider;
        private readonly LanguageServerWorkspace _workspace;
        private DiagnosticNotifier _diagnosticNotifier;

        private readonly Serilog.Core.Logger _logger;
        private readonly LogLevel _minLogLevel;

        private LanguageServerHost(
            MefHostServices exportProvider,
            Serilog.Core.Logger logger,
            LogLevel minLogLevel)
        {
            _exportProvider = exportProvider;
            _logger = logger;
            _minLogLevel = minLogLevel;

            _workspace = new LanguageServerWorkspace(_exportProvider);
        }

        public static async Task<LanguageServerHost> Create(
            Stream input,
            Stream output,
            string logFilePath,
            LogLevel minLogLevel)
        {
            var exportProvider = CreateHostServices();

            var logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.File(logFilePath)
                .CreateLogger();

            var result = new LanguageServerHost(exportProvider, logger, minLogLevel);

            await result.InitializeAsync(input, output);

            return result;
        }

        private static MefHostServices CreateHostServices()
        {
            var assemblies = MefHostServices.DefaultAssemblies
                .Union(new[] { typeof(LanguageServerHost).GetTypeInfo().Assembly });

            return MefHostServices.Create(assemblies);
        }

        private async Task InitializeAsync(Stream input, Stream output)
        {
            var documentSelector = new DocumentSelector(
                LanguageNames.AllLanguages
                    .Select(x => new DocumentFilter
                    {
                        Language = x.ToLowerInvariant()
                    }));

            _server = await OmniSharp.Extensions.LanguageServer.Server.LanguageServer.From(options => options
                .WithInput(input)
                .WithOutput(output)
                .ConfigureLogging(x => x
                    .AddSerilog(_logger)
                    .AddLanguageProtocolLogging()
                    .SetMinimumLevel(_minLogLevel))
                .AddHandler(new TextDocumentSyncHandler(_workspace, documentSelector))
                .AddHandler(new CompletionHandler(_workspace, documentSelector))
                .AddHandler(new DefinitionHandler(_workspace, documentSelector))
                .AddHandler(new WorkspaceSymbolsHandler(_workspace))
                .AddHandler(new DocumentHighlightHandler(_workspace, documentSelector))
                .AddHandler(new DocumentSymbolsHandler(_workspace, documentSelector))
                .AddHandler(new HoverHandler(_workspace, documentSelector))
                .AddHandler(new SignatureHelpHandler(_workspace, documentSelector)));

            var diagnosticService = _workspace.Services.GetService<IDiagnosticService>();
            _diagnosticNotifier = new DiagnosticNotifier(_server, diagnosticService);
        }

        public Task WaitForExit => _server.WaitForExit;

        public void Dispose()
        {
            _diagnosticNotifier?.Dispose();

            _server.Dispose();

            _logger.Dispose();
        }
    }
}
