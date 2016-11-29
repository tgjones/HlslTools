using System.ComponentModel.Composition;
using ShaderTools.Hlsl.Compilation;
using ShaderTools.Hlsl.Syntax;
using ShaderTools.Hlsl.Text;

namespace ShaderTools.VisualStudio.Hlsl.Navigation.GoToDefinitionProviders
{
    [Export(typeof(IGoToDefinitionProvider))]
    internal sealed class MacroReferenceGoToDefinitionProvider : GoToDefinitionProvider<SyntaxToken>
    {
        protected override TextSpan? CreateTargetSpan(SemanticModel semanticModel, SourceLocation position, SyntaxToken node)
        {
            if (node.MacroReference == null)
                return null;

            var nameToken = node.MacroReference.NameToken;

            if (!nameToken.SourceRange.ContainsOrTouches(position))
                return null;

            if (!nameToken.Span.IsInRootFile)
                return null;

            return node.MacroReference.DefineDirective.MacroName.Span;
        }
    }
}