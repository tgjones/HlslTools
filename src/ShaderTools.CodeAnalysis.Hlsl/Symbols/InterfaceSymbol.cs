using System.Collections.Immutable;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Symbols;
using ShaderTools.CodeAnalysis.Syntax;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.Hlsl.Symbols
{
    public sealed class InterfaceSymbol : TypeSymbol, INamedTypeSymbol
    {
        public override SyntaxTreeBase SourceTree { get; }
        public override ImmutableArray<SourceRange> Locations { get; }
        public override ImmutableArray<SyntaxNodeBase> DeclaringSyntaxNodes { get; }

        internal InterfaceSymbol(InterfaceTypeSyntax syntax, Symbol parent)
            : base(SymbolKind.Interface, syntax.Name.Text, string.Empty, parent)
        {
            SourceTree = syntax.SyntaxTree;
            Locations = ImmutableArray.Create(syntax.Name.SourceRange);
            DeclaringSyntaxNodes = ImmutableArray.Create((SyntaxNodeBase) syntax);
        }
    }
}