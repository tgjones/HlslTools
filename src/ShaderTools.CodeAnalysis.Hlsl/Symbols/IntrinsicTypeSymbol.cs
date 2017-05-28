using System.Collections.Immutable;
using ShaderTools.CodeAnalysis.Symbols;
using ShaderTools.CodeAnalysis.Syntax;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.Hlsl.Symbols
{
    public abstract class IntrinsicTypeSymbol : TypeSymbol
    {
        public override SyntaxTreeBase SourceTree { get; } = null;
        public override ImmutableArray<SourceRange> Locations { get; } = ImmutableArray<SourceRange>.Empty;

        internal IntrinsicTypeSymbol(SymbolKind kind, string name, string documentation)
            : base(kind, name, documentation, null)
        {
            
        }
    }
}