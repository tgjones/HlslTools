using System.ComponentModel.Composition;
using HlslTools.Compilation;
using HlslTools.Syntax;

namespace HlslTools.VisualStudio.IntelliSense.QuickInfo.QuickInfoModelProviders
{
    [Export(typeof(IQuickInfoModelProvider))]
    internal sealed class MacroReferenceQuickInfoModelProvider : QuickInfoModelProvider<SyntaxToken>
    {
        public override int Priority { get; } = 1;

        protected override QuickInfoModel CreateModel(SemanticModel semanticModel, SourceLocation position, SyntaxToken node)
        {
            if (node.MacroReference == null)
                return null;

            var nameToken = node.MacroReference.NameToken;

            if (!nameToken.SourceRange.ContainsOrTouches(position))
                return null;

            if (!nameToken.Span.IsInRootFile)
                return null;

            return new QuickInfoModel(semanticModel, nameToken.Span, $"(macro reference) {node.MacroReference.DefineDirective.ToString(true)}");
        }
    }
}