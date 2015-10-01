using System.Collections.Generic;
using HlslTools.Compilation;
using HlslTools.Syntax;

namespace HlslTools.VisualStudio.IntelliSense.Completion.CompletionProviders
{
    internal interface ICompletionProvider
    {
        IEnumerable<CompletionItem> GetItems(SemanticModel semanticModel, SourceLocation position);
    }
}