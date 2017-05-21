using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Symbols;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.Hlsl.Symbols
{
    public sealed class SourceVariableSymbol : VariableSymbol
    {
        public override SourceRange? Location { get; }

        internal SourceVariableSymbol(VariableDeclaratorSyntax syntax, Symbol parent, TypeSymbol valueType)
            : base(SymbolKind.Variable, syntax.Identifier.Text, string.Empty, parent, valueType)
        {
            Location = syntax.Identifier.SourceRange;
        }
    }
}