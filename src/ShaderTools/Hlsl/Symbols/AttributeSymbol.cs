using ShaderTools.Core.Symbols;

namespace ShaderTools.Hlsl.Symbols
{
    public sealed class AttributeSymbol : InvocableSymbol
    {
        internal AttributeSymbol(string name, string documentation)
            : base(SymbolKind.Attribute, name, documentation, null, TypeFacts.Missing)
        {
        }
    }
}