using System.ComponentModel.Composition;
using ShaderTools.Hlsl.Compilation;
using ShaderTools.Hlsl.Syntax;

namespace ShaderTools.VisualStudio.Hlsl.IntelliSense.QuickInfo.QuickInfoModelProviders
{
    [Export(typeof(IQuickInfoModelProvider))]
    internal sealed class FunctionDefinitionQuickInfoModelProvider : QuickInfoModelProvider<FunctionDefinitionSyntax>
    {
        protected override QuickInfoModel CreateModel(SemanticModel semanticModel, SourceLocation position, FunctionDefinitionSyntax node)
        {
            var actualName = node.Name.GetUnqualifiedName().Name;

            if (!actualName.SourceRange.ContainsOrTouches(position))
                return null;

            if (!actualName.Span.IsInRootFile)
                return null;

            var symbol = semanticModel.GetDeclaredSymbol(node);
            if (symbol == null)
                return null;

            return QuickInfoModel.ForSymbol(semanticModel, actualName.Span, symbol);
        }
    }
}