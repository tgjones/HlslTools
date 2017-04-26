using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Symbols;

namespace ShaderTools.CodeAnalysis.Hlsl.Symbols
{
    public sealed class ConstantBufferSymbol : ContainerSymbol
    {
        public ConstantBufferSyntax Syntax { get; }

        internal ConstantBufferSymbol(ConstantBufferSyntax syntax, Symbol parent)
            : base(SymbolKind.ConstantBuffer, syntax.Name.Text, string.Empty, parent)
        {
            Syntax = syntax;
        }
    }
}