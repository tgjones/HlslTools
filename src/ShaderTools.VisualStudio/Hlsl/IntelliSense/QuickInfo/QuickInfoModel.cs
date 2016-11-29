using ShaderTools.Hlsl.Compilation;
using ShaderTools.Hlsl.Symbols;
using ShaderTools.Hlsl.Symbols.Markup;
using ShaderTools.Hlsl.Syntax;
using ShaderTools.Hlsl.Text;
using ShaderTools.VisualStudio.Hlsl.Glyphs;

namespace ShaderTools.VisualStudio.Hlsl.IntelliSense.QuickInfo
{
    internal sealed class QuickInfoModel
    {
        public static QuickInfoModel ForSymbol(SemanticModel semanticModel, TextSpan span, Symbol symbol)
        {
            var glyph = symbol.GetGlyph();
            var symbolMarkup = SymbolMarkup.ForSymbol(symbol);
            return new QuickInfoModel(semanticModel, span, glyph, symbolMarkup, symbol.Documentation);
        }

        public static QuickInfoModel ForMacroDefinition(SemanticModel semanticModel, TextSpan span, DefineDirectiveTriviaSyntax macroDefinition)
        {
            var glyph = Glyph.Macro;
            var symbolMarkup = new SymbolMarkup(new[] { new SymbolMarkupToken(SymbolMarkupKind.PlainText, $"(macro definition) {macroDefinition}") });
            return new QuickInfoModel(semanticModel, span, glyph, symbolMarkup, string.Empty);
        }

        public static QuickInfoModel ForMacroReference(SemanticModel semanticModel, TextSpan span, MacroReference macroReference)
        {
            var glyph = Glyph.Macro;
            var symbolMarkup = new SymbolMarkup(new[] { new SymbolMarkupToken(SymbolMarkupKind.PlainText, $"(macro reference) {macroReference.DefineDirective.ToString(true)}") });
            return new QuickInfoModel(semanticModel, span, glyph, symbolMarkup, string.Empty);
        }

        private QuickInfoModel(SemanticModel semanticModel, TextSpan span, Glyph glyph, SymbolMarkup markup, string documentation)
        {
            SemanticModel = semanticModel;
            Span = span;
            Glyph = glyph;
            Markup = markup;
            Documentation = documentation;
        }

        public SemanticModel SemanticModel { get; }
        public TextSpan Span { get; }
        public Glyph Glyph { get; }
        public SymbolMarkup Markup { get; }
        public string Documentation { get; }
    }
}