using System.Collections.Generic;
using ShaderTools.Core.Syntax;
using ShaderTools.Hlsl.Compilation;

namespace ShaderTools.Editor.VisualStudio.Hlsl.IntelliSense.Completion.CompletionProviders
{
    internal interface ICompletionProvider
    {
        IEnumerable<CompletionItem> GetItems(SemanticModel semanticModel, SourceLocation position);
    }
}