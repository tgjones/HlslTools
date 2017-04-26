using ShaderTools.CodeAnalysis.Symbols;

namespace ShaderTools.CodeAnalysis.Hlsl.Symbols
{
    public sealed class AttributeSymbol : InvocableSymbol
    {
        internal AttributeSymbol(string name, string documentation)
            : base(SymbolKind.Attribute, name, documentation, null, TypeFacts.Missing)
        {
        }
    }
}