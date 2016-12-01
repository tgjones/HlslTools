using System.Threading;
using Microsoft.VisualStudio.Text;
using ShaderTools.Hlsl.Compilation;
using ShaderTools.VisualStudio.Core.Parsing;
using ShaderTools.VisualStudio.Hlsl.Util.Extensions;

namespace ShaderTools.VisualStudio.Hlsl.Parsing
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
            // Force creation of SemanticModel.
            SemanticModel semanticModel;
            return snapshot.TryGetSemanticModel(cancellationToken, out semanticModel);
        }
    }
}