using System.Collections.Immutable;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Symbols;
using ShaderTools.CodeAnalysis.Syntax;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.Hlsl.Symbols
{
    public sealed class TechniqueSymbol : Symbol, INamespaceOrTypeSymbol
    {
        public override SyntaxTreeBase SourceTree { get; }
        public override ImmutableArray<SourceRange> Locations { get; }
        public override ImmutableArray<SyntaxNodeBase> DeclaringSyntaxNodes { get; }

        public ImmutableArray<ISymbol> GetMembers() => ImmutableArray<ISymbol>.Empty; // TODO

        internal TechniqueSymbol(TechniqueSyntax syntax)
            : base(SymbolKind.Technique, syntax.Name?.Text, string.Empty, null)
        {
            SourceTree = syntax.SyntaxTree;
            Locations = syntax.Name != null
                ? ImmutableArray.Create(syntax.Name.SourceRange)
                : ImmutableArray<SourceRange>.Empty;
            DeclaringSyntaxNodes = ImmutableArray.Create((SyntaxNodeBase) syntax);
        }
    }
}