using System.Collections.Generic;

namespace ShaderTools.CodeAnalysis.Symbols.Markup
{
    internal static class SymbolMarkupBuilder
    {
        private static void Append(this ICollection<SymbolMarkupToken> markup, SymbolMarkupKind kind, string text)
        {
            markup.Add(new SymbolMarkupToken(kind, text));
        }

        public static void AppendName(this ICollection<SymbolMarkupToken> markup, SymbolMarkupKind kind, string name)
        {
            var displayName = !string.IsNullOrEmpty(name)
                ? name
                : "?";
            markup.Append(kind, displayName);
        }

        public static void AppendKeyword(this ICollection<SymbolMarkupToken> markup, string text)
        {
            markup.Append(SymbolMarkupKind.Keyword, text);
        }

        public static void AppendSpace(this ICollection<SymbolMarkupToken> markup)
        {
            markup.Append(SymbolMarkupKind.Whitespace, " ");
        }

        public static void AppendPunctuation(this ICollection<SymbolMarkupToken> markup, string text)
        {
            markup.Append(SymbolMarkupKind.Punctuation, text);
        }

        public static void AppendPlainText(this ICollection<SymbolMarkupToken> markup, string text)
        {
            markup.AppendName(SymbolMarkupKind.PlainText, text);
        }
    }
}