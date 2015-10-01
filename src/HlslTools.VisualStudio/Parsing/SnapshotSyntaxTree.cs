using HlslTools.Syntax;
using Microsoft.VisualStudio.Text;

namespace HlslTools.VisualStudio.Parsing
{
    internal sealed class SnapshotSyntaxTree
    {
        public ITextSnapshot Snapshot { get; }
        public SyntaxTree SyntaxTree { get; }

        public SnapshotSyntaxTree(ITextSnapshot snapshot, SyntaxTree syntaxTree)
        {
            Snapshot = snapshot;
            SyntaxTree = syntaxTree;
        }
    }
}