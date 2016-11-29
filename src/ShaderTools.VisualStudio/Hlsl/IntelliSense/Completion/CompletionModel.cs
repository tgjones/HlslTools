using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.VisualStudio.Text;
using ShaderTools.Core.Text;
using ShaderTools.Hlsl.Compilation;
using ShaderTools.Hlsl.Text;

namespace ShaderTools.VisualStudio.Hlsl.IntelliSense.Completion
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