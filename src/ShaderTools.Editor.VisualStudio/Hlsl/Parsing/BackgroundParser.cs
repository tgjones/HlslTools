using System.Threading;
using Microsoft.VisualStudio.Text;
using ShaderTools.CodeAnalysis.Hlsl.Compilation;
using ShaderTools.Editor.VisualStudio.Core.Parsing;
using ShaderTools.Editor.VisualStudio.Hlsl.Util.Extensions;

namespace ShaderTools.Editor.VisualStudio.Hlsl.Parsing
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