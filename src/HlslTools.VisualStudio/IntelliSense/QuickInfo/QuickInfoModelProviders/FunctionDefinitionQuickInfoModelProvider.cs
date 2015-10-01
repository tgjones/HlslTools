using System.ComponentModel.Composition;
using HlslTools.Compilation;
using HlslTools.Syntax;
using HlslTools.VisualStudio.Util.SyntaxOutput;

namespace HlslTools.VisualStudio.IntelliSense.QuickInfo.QuickInfoModelProviders
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

            return new QuickInfoModel(semanticModel, actualName.Span, $"(function) {node.GetDescription(true, true)}");
        }
    }
}