using System.Collections.Immutable;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Symbols;
using ShaderTools.CodeAnalysis.Syntax;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.Hlsl.Symbols
{
    public sealed class TechniqueSymbol : Symbol
    {
        public override SyntaxTreeBase SourceTree { get; }
        public override ImmutableArray<SourceRange> Locations { get; }

        internal TechniqueSymbol(TechniqueSyntax syntax)
            : base(SymbolKind.Technique, syntax.Name?.Text, string.Empty, null)
        {
            SourceTree = syntax.SyntaxTree;
            Locations = syntax.Name != null
                ? ImmutableArray.Create(syntax.Name.SourceRange)
                : ImmutableArray<SourceRange>.Empty;
        }
    }
}