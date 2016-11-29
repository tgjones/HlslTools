using ShaderTools.Hlsl.Symbols;
using ShaderTools.VisualStudio.Hlsl.Glyphs;

namespace ShaderTools.VisualStudio.Hlsl.IntelliSense.Completion
{
    internal sealed class CompletionItem
    {
        public CompletionItem(string displayText, string insertionText, string description, Glyph? glyph)
            : this(displayText, insertionText, description, glyph, null)
        {
        }

        public CompletionItem(string displayText, string insertionText, string description, bool isBuilder)
            : this(displayText, insertionText, description, null, null, isBuilder)
        {
        }

        public CompletionItem(string displayText, string insertionText, string description, Glyph? glyph, Symbol symbol)
            : this(displayText, insertionText, description, glyph, symbol, false)
        {
        }

        public CompletionItem(string displayText, string insertionText, string description, Glyph? glyph, Symbol symbol, bool isBuilder)
        {
            DisplayText = displayText;
            InsertionText = insertionText;
            Description = description;
            Glyph = glyph;
            Symbol = symbol;
            IsBuilder = isBuilder;
        }

        public string DisplayText { get; }
        public string InsertionText { get; }
        public string Description { get; }
        public Glyph? Glyph { get; }
        public Symbol Symbol { get; }
        public bool IsBuilder { get; }
    }
}