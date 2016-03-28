using HlslTools.Compilation;
using HlslTools.Symbols;
using HlslTools.Symbols.Markup;
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

        // TODO: Remove this.
        public QuickInfoModel(SemanticModel semanticModel, TextSpan span, string text)
        {
            SemanticModel = semanticModel;
            Span = span;
            Text = text;
        }

        public QuickInfoModel(SemanticModel semanticModel, TextSpan span, Glyph glyph, SymbolMarkup markup, string documentation)
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