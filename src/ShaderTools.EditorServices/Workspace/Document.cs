using System.Threading;
using System.Threading.Tasks;
using ShaderTools.CodeAnalysis.Compilation;
using ShaderTools.CodeAnalysis.Syntax;
using ShaderTools.CodeAnalysis.Text;
using ShaderTools.EditorServices.Utility;
using ShaderTools.EditorServices.Workspace.Host;

namespace ShaderTools.EditorServices.Workspace
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
            return _lazySemanticModel.GetValueAsync(cancellationToken);
        }

        public Document WithId(DocumentId documentId)
        {
            return new Document(_languageServices, documentId, SourceText);
        }

        public Document WithText(SourceText newText)
        {
            return new Document(_languageServices, Id, newText);
        }
    }
}
