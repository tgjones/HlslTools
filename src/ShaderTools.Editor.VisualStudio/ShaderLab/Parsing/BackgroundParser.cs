using System.Threading;
using Microsoft.VisualStudio.Text;
using ShaderTools.Editor.VisualStudio.Core.Parsing;
using ShaderTools.Editor.VisualStudio.ShaderLab.Util.Extensions;

namespace ShaderTools.Editor.VisualStudio.ShaderLab.Parsing
{
    internal sealed class BackgroundParser : BackgroundParserBase
    {
        public BackgroundParser(ITextBuffer textBuffer)
            : base(textBuffer)
        {
        }

        protected override void CreateSyntaxTree(ITextSnapshot snapshot, CancellationToken cancellationToken)
        {
            // Force creation of SyntaxTree.
            snapshot.GetSyntaxTree(cancellationToken);
        }

        protected override bool TryCreateSemanticModel(ITextSnapshot snapshot, CancellationToken cancellationToken)
        {
            // TODO
            return false;
        }
    }
}
