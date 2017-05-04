using System.ComponentModel.Composition;
using ShaderTools.CodeAnalysis.Hlsl.Compilation;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.Editor.VisualStudio.Hlsl.Navigation.GoToDefinitionProviders
{
    [Export(typeof(IGoToDefinitionProvider))]
    internal sealed class MacroReferenceGoToDefinitionProvider : GoToDefinitionProvider<SyntaxToken>
    {
        protected override SourceFileSpan? CreateTargetSpan(SemanticModel semanticModel, SourceLocation position, SyntaxToken node)
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