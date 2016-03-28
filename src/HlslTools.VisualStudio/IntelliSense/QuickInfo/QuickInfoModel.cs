using HlslTools.Compilation;
using HlslTools.Symbols;
using HlslTools.Symbols.Markup;
using HlslTools.Syntax;
using HlslTools.Text;
using HlslTools.VisualStudio.Glyphs;

namespace HlslTools.VisualStudio.IntelliSense.QuickInfo
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
            Text = markup.ToString();
            Documentation = documentation;
        }

        public SemanticModel SemanticModel { get; }
        public TextSpan Span { get; }
        public Glyph Glyph { get; }
        public string Text { get; }
        public string Documentation { get; }
    }
}