using ShaderTools.CodeAnalysis.Symbols;

namespace ShaderTools.CodeAnalysis.Hlsl.Symbols
{
    public sealed class TechniqueSymbol : Symbol
    {
        internal TechniqueSymbol(string name)
            : base(SymbolKind.Technique, name, string.Empty, null)
        {
        }
    }
}