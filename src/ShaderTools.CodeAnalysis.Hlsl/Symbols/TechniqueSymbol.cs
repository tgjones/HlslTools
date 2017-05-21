using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Symbols;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.Hlsl.Symbols
{
    public sealed class TechniqueSymbol : Symbol
    {
        public override SourceRange? Location { get; }

        internal TechniqueSymbol(TechniqueSyntax syntax)
            : base(SymbolKind.Technique, syntax.Name.Text, string.Empty, null)
        {
            Location = syntax.Name.SourceRange;
        }
    }
}