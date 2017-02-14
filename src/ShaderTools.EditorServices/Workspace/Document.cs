using System.Threading;
using System.Threading.Tasks;
using ShaderTools.Core.Compilation;
using ShaderTools.Core.Syntax;
using ShaderTools.Core.Text;
using ShaderTools.EditorServices.Utility;

namespace ShaderTools.EditorServices.Workspace
{
    /// <summary>
    /// Contains the details and contents of an open document.
    /// </summary>
    public abstract class Document
    {
        private readonly AsyncLazy<SyntaxTreeBase> _lazySyntaxTree;
        private readonly AsyncLazy<SemanticModelBase> _lazySemanticModel;

        public SourceText SourceText { get; }

        /// <summary>
        /// Gets a unique string that identifies this file.  At this time,
        /// this property returns a normalized version of the value stored
        /// in the FilePath property.
        /// </summary>
        public string Id => FilePath.ToLower();

        /// <summary>
        /// Gets the path at which this file resides.
        /// </summary>
        public string FilePath => SourceText.Filename;

        /// <summary>
        /// Gets the path which the editor client uses to identify this file.
        /// </summary>
        public string ClientFilePath { get; }

        /// <summary>
        /// Gets a boolean that determines whether this file is
        /// in-memory or not (either unsaved or non-file content).
        /// </summary>
        public bool IsInMemory { get; }

        public Document(SourceText sourceText, string clientFilePath)
        {
            SourceText = sourceText;
            ClientFilePath = clientFilePath;
            IsInMemory = Workspace.IsPathInMemory(sourceText.Filename);

            _lazySyntaxTree = new AsyncLazy<SyntaxTreeBase>(ct => Task.Run(() => Compile(sourceText, ct), ct), true);
            _lazySemanticModel = new AsyncLazy<SemanticModelBase>(ct => Task.Run(async () =>
            {
                var syntaxTree = await GetSyntaxTreeAsync(ct);
                return CreateSemanticModel(syntaxTree, ct);
            }, ct), true);
        }

        protected abstract SyntaxTreeBase Compile(SourceText sourceText, CancellationToken cancellationToken);
        protected abstract SemanticModelBase CreateSemanticModel(SyntaxTreeBase syntaxTree, CancellationToken cancellationToken);

        public Task<SyntaxTreeBase> GetSyntaxTreeAsync(CancellationToken cancellationToken)
        {
            return _lazySyntaxTree.GetValueAsync(cancellationToken);
        }

        public Task<SemanticModelBase> GetSemanticModelAsync(CancellationToken cancellationToken)
        {
            return _lazySemanticModel.GetValueAsync(cancellationToken);
        }

        public abstract Document WithSourceText(SourceText sourceText);
    }
}
