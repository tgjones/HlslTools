using ShaderTools.Core.Symbols;

namespace ShaderTools.Hlsl.Symbols
{
    public sealed class TechniqueSymbol : Symbol
    {
        internal TechniqueSymbol(string name)
            : base(SymbolKind.Technique, name, string.Empty, null)
        {
        }
    }
}