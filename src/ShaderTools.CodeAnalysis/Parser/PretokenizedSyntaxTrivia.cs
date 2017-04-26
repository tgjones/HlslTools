using System.Collections.Immutable;

namespace ShaderTools.CodeAnalysis.Parser
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
