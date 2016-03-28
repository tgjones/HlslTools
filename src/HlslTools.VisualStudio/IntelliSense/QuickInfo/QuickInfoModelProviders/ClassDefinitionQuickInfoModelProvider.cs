using System.ComponentModel.Composition;
using HlslTools.Compilation;
using HlslTools.Syntax;

namespace HlslTools.VisualStudio.IntelliSense.QuickInfo.QuickInfoModelProviders
{
    [Export(typeof(IQuickInfoModelProvider))]
    internal sealed class ClassDefinitionQuickInfoModelProvider : QuickInfoModelProvider<ClassTypeSyntax>
    {
        protected override QuickInfoModel CreateModel(SemanticModel semanticModel, SourceLocation position, ClassTypeSyntax node)
        {
            if (!node.Name.SourceRange.ContainsOrTouches(position))
                return null;

            if (!node.Name.Span.IsInRootFile)
                return null;

            var symbol = semanticModel.GetDeclaredSymbol(node);
            if (symbol == null)
                return null;

            return QuickInfoModel.ForSymbol(semanticModel, node.Name.Span, symbol);
        }
    }
}