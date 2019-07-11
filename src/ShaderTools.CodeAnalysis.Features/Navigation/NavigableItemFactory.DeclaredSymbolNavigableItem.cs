using System.Collections.Immutable;
using ShaderTools.CodeAnalysis.Shared.Extensions;
using ShaderTools.CodeAnalysis.Symbols;
using ShaderTools.CodeAnalysis.Text;
using TaggedText = Microsoft.CodeAnalysis.TaggedText;

namespace ShaderTools.CodeAnalysis.Navigation
{
    internal partial class NavigableItemFactory
    {
        internal class DeclaredSymbolNavigableItem : INavigableItem
        {
            private readonly ISymbol _declaredSymbol;

            public Document Document { get; }

            public ImmutableArray<TaggedText> DisplayTaggedParts => _declaredSymbol.ToMarkup(SymbolDisplayFormat.NavigateTo).Tokens.ToTaggedText();

            public Glyph Glyph => _declaredSymbol.GetGlyph();

            public SourceFileSpan SourceSpan { get; }

            public ImmutableArray<INavigableItem> ChildItems => ImmutableArray<INavigableItem>.Empty;
            public bool DisplayFileLocation => false;

            /// <summary>
            /// DeclaredSymbolInfos always come from some actual declaration in source.  So they're
            /// never implicitly declared.
            /// </summary>
            public bool IsImplicitlyDeclared => false;

            public DeclaredSymbolNavigableItem(Document document, ISymbol symbol, SourceFileSpan sourceSpan)
            {
                Document = document;
                _declaredSymbol = symbol;
                SourceSpan = sourceSpan;
            }
        }
    }
}