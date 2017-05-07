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

        public Task<SemanticModelBase> GetSemanticModelAsync(CancellationToken cancellationToken)
        {
            var optionsService = _languageServices.GetRequiredService<IOptionsService>();
            if (!optionsService.EnableIntelliSense)
                return Task.FromResult<SemanticModelBase>(null);

            return _lazySemanticModel.GetValueAsync(cancellationToken);
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
    }
}
