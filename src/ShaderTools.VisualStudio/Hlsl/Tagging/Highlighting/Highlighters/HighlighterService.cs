using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Composition;

namespace ShaderTools.VisualStudio.Hlsl.Tagging.Highlighting.Highlighters
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