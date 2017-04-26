using System.Collections.Generic;
using ShaderTools.CodeAnalysis.Hlsl.Compilation;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.Editor.VisualStudio.Hlsl.IntelliSense.Completion.CompletionProviders
{
    internal interface ICompletionProvider
    {
        IEnumerable<CompletionItem> GetItems(SemanticModel semanticModel, SourceLocation position);
    }
}