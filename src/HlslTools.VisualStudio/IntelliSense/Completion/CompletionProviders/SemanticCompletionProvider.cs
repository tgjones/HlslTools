using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using HlslTools.Compilation;
using HlslTools.Symbols;
using HlslTools.Syntax;
using HlslTools.VisualStudio.Glyphs;

namespace HlslTools.VisualStudio.IntelliSense.Completion.CompletionProviders
{
    [Export(typeof(ICompletionProvider))]
    internal sealed class SemanticCompletionProvider : CompletionProvider<SemanticSyntax>
    {
        protected override IEnumerable<CompletionItem> GetItems(SemanticModel semanticModel, SourceLocation position, SemanticSyntax node)
        {
            if (node.ColonToken.IsMissing || position < node.ColonToken.SourceRange.End)
                return Enumerable.Empty<CompletionItem>();

            return semanticModel
                .LookupSymbols(node.Semantic.SourceRange.Start)
                .OfType<SemanticSymbol>()
                .Select(x => new CompletionItem($"{x.Name}{(x.AllowsMultiple ? "[n]" : "")}", x.Name, $"(semantic) {x.Name}\n{x.FullDescription}", Glyph.Semantic));
        }
    }
}