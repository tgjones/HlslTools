using ShaderTools.CodeAnalysis.Symbols;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.Hlsl.Symbols
{
    public sealed class AttributeSymbol : InvocableSymbol
    {
        public override SourceRange? Location => null;

        internal AttributeSymbol(string name, string documentation)
            : base(SymbolKind.Attribute, name, documentation, null, TypeFacts.Missing)
        {
        }
    }
}