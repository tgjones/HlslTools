using HlslTools.Compilation;
using HlslTools.Syntax;
using Microsoft.VisualStudio.Text;

namespace HlslTools.VisualStudio.Parsing
{
    internal sealed class SnapshotSyntaxTree
    {
        public ITextSnapshot Snapshot { get; }
        public SyntaxTree SyntaxTree { get; }
        public SemanticModel SemanticModel { get; }

        public SnapshotSyntaxTree(ITextSnapshot snapshot, SyntaxTree syntaxTree, SemanticModel semanticModel)
        {
            Snapshot = snapshot;
            SyntaxTree = syntaxTree;
            SemanticModel = semanticModel;
        }
    }
}