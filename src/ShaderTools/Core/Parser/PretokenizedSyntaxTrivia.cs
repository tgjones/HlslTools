using System.Collections.Immutable;
using ShaderTools.Core.Diagnostics;

namespace ShaderTools.Core.Parser
{
    internal sealed class PretokenizedSyntaxTrivia : PretokenizedSyntaxNode
    {
        public PretokenizedSyntaxTrivia(
            ushort rawKind, 
            string text, 
            ImmutableArray<PretokenizedDiagnostic> diagnostics) 
            : base(rawKind, text, diagnostics)
        {
            FullWidth = text.Length;
        }
    }
}
