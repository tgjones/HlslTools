using System.Collections.Generic;
using System.Collections.Immutable;
using ShaderTools.CodeAnalysis.Symbols;
using ShaderTools.Utilities.PooledObjects;
using TaggedText = Microsoft.CodeAnalysis.TaggedText;

namespace ShaderTools.CodeAnalysis.FindUsages
{
    internal static class DefinitionItemExtensions
    {
        public static DefinitionItem ToDefinitionItem(
            this ISymbol definition,
            Workspace workspace,
            HashSet<DocumentSpan> uniqueSpans = null)
        {
            var displayParts = definition.ToMarkup(SymbolDisplayFormat.QuickInfo).Tokens.ToTaggedText();
            var nameDisplayParts = ImmutableArray<TaggedText>.Empty; // definition.ToDisplayParts(s_namePartsFormat).ToTaggedText();

            var tags = ImmutableArray<string>.Empty; // GlyphTags.GetTags(definition.GetGlyph());
            var sourceLocations = ArrayBuilder<DocumentSpan>.GetInstance();

            foreach (var location in definition.Locations)
            {
                var sourceFileSpan = definition.SourceTree.GetSourceFileSpan(location);

                var document = workspace.CurrentDocuments.GetDocument(definition.SourceTree);
                if (document != null)
                {
                    var documentLocation = new DocumentSpan(document, sourceFileSpan);
                    if (sourceLocations.Count == 0)
                    {
                        sourceLocations.Add(documentLocation);
                    }
                    else
                    {
                        if (uniqueSpans == null ||
                            uniqueSpans.Add(documentLocation))
                        {
                            sourceLocations.Add(documentLocation);
                        }
                    }
                }
            }

            return DefinitionItem.Create(
                tags, displayParts, sourceLocations.ToImmutableAndFree(),
                nameDisplayParts);
        }
    }
}
