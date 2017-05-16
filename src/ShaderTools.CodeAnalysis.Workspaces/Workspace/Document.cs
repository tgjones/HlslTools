using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ShaderTools.CodeAnalysis.Compilation;
using ShaderTools.CodeAnalysis.Host;
using ShaderTools.CodeAnalysis.Options;
using ShaderTools.CodeAnalysis.Syntax;
using ShaderTools.CodeAnalysis.Text;
using ShaderTools.Utilities.Threading;

namespace ShaderTools.CodeAnalysis
{
    /// <summary>
    /// Contains the details and contents of an open document.
    /// </summary>
    public sealed class Document
    {
        private readonly HostLanguageServices _languageServices;
        private readonly AsyncLazy<SyntaxTreeBase> _lazySyntaxTree;
        private readonly AsyncLazy<SemanticModelBase> _lazySemanticModel;

        /// <summary>
        /// Gets a unique string that identifies this file.  At this time,
        /// this property returns a normalized version of the value stored
        /// in the FilePath property.
        /// </summary>
        public DocumentId Id { get; }

        public SourceText SourceText { get; }

        /// <summary>
        /// Gets the path at which this file resides.
        /// </summary>
        public string FilePath => Id.OriginalFilePath;

        public string Name => (FilePath != null) ? Path.GetFileName(FilePath) : "[NoName]";

        public string Language => _languageServices.Language;

        public HostLanguageServices LanguageServices => _languageServices;

        public Workspace Workspace => _languageServices.WorkspaceServices.Workspace;

        internal Document(HostLanguageServices languageServices, DocumentId documentId, SourceText sourceText)
        {
            _languageServices = languageServices;

            Id = documentId;
            SourceText = sourceText;

            _lazySyntaxTree = new AsyncLazy<SyntaxTreeBase>(ct => Task.Run(() =>
            {
                var syntaxTreeFactory = languageServices.GetRequiredService<ISyntaxTreeFactoryService>();

                return syntaxTreeFactory.ParseSyntaxTree(sourceText, ct);
            }, ct), true);

            _lazySemanticModel = new AsyncLazy<SemanticModelBase>(ct => Task.Run(async () =>
            {
                var syntaxTree = await GetSyntaxTreeAsync(ct).ConfigureAwait(false);

                var compilationFactory = languageServices.GetRequiredService<ICompilationFactoryService>();

                return compilationFactory.CreateCompilation(syntaxTree).GetSemanticModelBase(ct);
            }, ct), true);
        }

        public Task<SyntaxTreeBase> GetSyntaxTreeAsync(CancellationToken cancellationToken)
        {
            return _lazySyntaxTree.GetValueAsync(cancellationToken);
        }

        public async Task<SemanticModelBase> GetSemanticModelAsync(CancellationToken cancellationToken)
        {
            var options = await GetOptionsAsync(cancellationToken).ConfigureAwait(false);

            if (!options.GetOption(FeatureOnOffOptions.IntelliSense))
                return null;

            return await _lazySemanticModel.GetValueAsync(cancellationToken);
        }

        public Document WithId(DocumentId documentId)
        {
            return new Document(_languageServices, documentId, SourceText);
        }

        /// <summary>
        /// Creates a new instance of this document updated to have the text specified.
        /// </summary>
        public Document WithText(SourceText newText)
        {
            return new Document(_languageServices, Id, newText);
        }

        private AsyncLazy<DocumentOptionSet> _cachedOptions;

        /// <summary>
        /// Returns the options that should be applied to this document. This consists of global options from <see cref="Solution.Options"/>,
        /// merged with any settings the user has specified at the document levels.
        /// </summary>
        /// <remarks>
        /// This method is async because this may require reading other files. In files that are already open, this is expected to be cheap and complete synchronously.
        /// </remarks>
        public Task<DocumentOptionSet> GetOptionsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (_cachedOptions == null)
            {
                var newAsyncLazy = new AsyncLazy<DocumentOptionSet>(async c =>
                {
                    var optionsService = Workspace.Services.GetRequiredService<IOptionService>();
                    var optionSet = await optionsService.GetUpdatedOptionSetForDocumentAsync(this, Workspace.Options, c).ConfigureAwait(false);
                    return new DocumentOptionSet(optionSet, Language);
                }, cacheResult: true);

                Interlocked.CompareExchange(ref _cachedOptions, newAsyncLazy, comparand: null);
            }

            return _cachedOptions.GetValueAsync(cancellationToken);
        }
    }
}
