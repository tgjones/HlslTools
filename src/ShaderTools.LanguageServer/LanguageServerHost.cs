using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
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
        private LanguageServerWorkspace _workspace;
        private DiagnosticNotifier _diagnosticNotifier;

        private readonly LoggerFactory _loggerFactory;
        private readonly Serilog.Core.Logger _logger;

        private LanguageServerHost(
            MefHostServices exportProvider,
            Serilog.Core.Logger logger,
            LoggerFactory loggerFactory)
        {
            _exportProvider = exportProvider;
            _logger = logger;
            _loggerFactory = loggerFactory;
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

            var loggerFactory = new LoggerFactory(
                ImmutableArray<ILoggerProvider>.Empty,
                new LoggerFilterOptions { MinLevel = minLogLevel });

            loggerFactory.AddSerilog(logger);

            var result = new LanguageServerHost(exportProvider, logger, loggerFactory);

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
            _server = await OmniSharp.Extensions.LanguageServer.Server.LanguageServer.From(options => options
                .WithInput(input)
                .WithOutput(output)
                .WithLoggerFactory(_loggerFactory)
                .OnInitialized(OnInitialized));

            var diagnosticService = _workspace.Services.GetService<IDiagnosticService>();
            _diagnosticNotifier = new DiagnosticNotifier(_server, diagnosticService);

            var documentSelector = new DocumentSelector(
                LanguageNames.AllLanguages
                    .Select(x => new DocumentFilter
                    {
                        Language = x.ToLowerInvariant()
                    }));

            var registrationOptions = new TextDocumentRegistrationOptions
            {
                DocumentSelector = documentSelector
            };

            _server.AddHandlers(
                new TextDocumentSyncHandler(_workspace, registrationOptions),
                new CompletionHandler(_workspace, registrationOptions),
                new DefinitionHandler(_workspace, registrationOptions),
                new WorkspaceSymbolsHandler(_workspace),
                new DocumentHighlightHandler(_workspace, registrationOptions),
                new DocumentSymbolsHandler(_workspace, registrationOptions),
                new HoverHandler(_workspace, registrationOptions),
                new SignatureHelpHandler(_workspace, registrationOptions));
        }

        private Task OnInitialized(ILanguageServer server, InitializeParams request, InitializeResult result)
        {
            _workspace = new LanguageServerWorkspace(_exportProvider, request.RootPath);

            return Task.CompletedTask;
        }

        public Task WaitForExit => _server.WaitForExit;

        public void Dispose()
        {
            _diagnosticNotifier?.Dispose();

            _server.Dispose();

            _logger.Dispose();
            _loggerFactory.Dispose();
        }
    }
}
