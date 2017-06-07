using System.Collections.Immutable;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Symbols;
using ShaderTools.CodeAnalysis.Syntax;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.Hlsl.Symbols
{
    public sealed class SourceVariableSymbol : VariableSymbol
    {
        public override SyntaxTreeBase SourceTree { get; }
        public override ImmutableArray<SourceRange> Locations { get; }
        public override ImmutableArray<SyntaxNodeBase> DeclaringSyntaxNodes { get; }

        internal SourceVariableSymbol(VariableDeclaratorSyntax syntax, Symbol parent, TypeSymbol valueType)
            : base(SymbolKind.Variable, syntax.Identifier.Text, string.Empty, parent, valueType)
        {
            SourceTree = syntax.SyntaxTree;
            Locations = ImmutableArray.Create(syntax.Identifier.SourceRange);
            DeclaringSyntaxNodes = ImmutableArray.Create((SyntaxNodeBase) syntax);
        }
    }
}