using System.ComponentModel.Composition;
using HlslTools.Compilation;
using HlslTools.Syntax;
using HlslTools.Text;

namespace HlslTools.VisualStudio.Navigation.GoToDefinitionProviders
{
    [Export(typeof(IGoToDefinitionProvider))]
    internal sealed class MacroReferenceGoToDefinitionProvider : GoToDefinitionProvider<SyntaxToken>
    {
        protected override TextSpan? CreateTargetSpan(SemanticModel semanticModel, SourceLocation position, SyntaxToken node)
        {
            if (!node.SourceRange.ContainsOrTouches(position))
                return null;

            if (!node.Span.IsInRootFile)
                return null;

            return node.MacroReference?.DefineDirective.MacroName.Span;
        }
    }
}