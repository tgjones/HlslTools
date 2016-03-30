using System.ComponentModel.Composition;
using HlslTools.Compilation;
using HlslTools.Syntax;

namespace HlslTools.VisualStudio.IntelliSense.QuickInfo.QuickInfoModelProviders
{
    [Export(typeof(IQuickInfoModelProvider))]
    internal sealed class ObjectTypeQuickInfoModelProvider : QuickInfoModelProvider<PredefinedObjectTypeSyntax>
    {
        protected override QuickInfoModel CreateModel(SemanticModel semanticModel, SourceLocation position, PredefinedObjectTypeSyntax node)
        {
            if (!node.ObjectTypeToken.SourceRange.ContainsOrTouches(position))
                return null;

            if (!node.ObjectTypeToken.Span.IsInRootFile)
                return null;

            var symbol = semanticModel.GetSymbol(node);
            if (symbol == null)
                return null;

            return QuickInfoModel.ForSymbol(semanticModel, node.ObjectTypeToken.Span, symbol);
        }
    }
}