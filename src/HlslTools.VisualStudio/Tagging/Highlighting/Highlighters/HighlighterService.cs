using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Composition;

namespace HlslTools.VisualStudio.Tagging.Highlighting.Highlighters
{
    [Export]
    internal sealed class HighlighterService
    {
        [ImportingConstructor]
        public HighlighterService([ImportMany] IEnumerable<IHighlighter> highlighters)
        {
            Highlighters = highlighters.ToImmutableArray();
        }

        public ImmutableArray<IHighlighter> Highlighters { get; }
    }
}