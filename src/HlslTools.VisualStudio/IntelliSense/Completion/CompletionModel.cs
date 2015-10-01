using System.Collections.Generic;
using System.Collections.Immutable;
using HlslTools.Compilation;
using HlslTools.Text;
using Microsoft.VisualStudio.Text;

namespace HlslTools.VisualStudio.IntelliSense.Completion
{
    internal sealed class CompletionModel
    {
        public CompletionModel(SemanticModel semanticModel, TextSpan applicableSpan, ITextSnapshot textSnapshot, IEnumerable<CompletionItem> items)
        {
            SemanticModel = semanticModel;
            ApplicableSpan = applicableSpan;
            TextSnapshot = textSnapshot;
            Items = items.ToImmutableArray();
        }

        public SemanticModel SemanticModel { get; }
        public TextSpan ApplicableSpan { get; }
        public ITextSnapshot TextSnapshot { get; set; }
        public ImmutableArray<CompletionItem> Items { get; }
    }
}