using System.ComponentModel.Composition;
using ShaderTools.CodeAnalysis.Hlsl.Compilation;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.Editor.VisualStudio.Hlsl.IntelliSense.QuickInfo.QuickInfoModelProviders
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

            if (!nameToken.FileSpan.IsInRootFile)
                return null;

            return QuickInfoModel.ForMacroReference(semanticModel, nameToken.FileSpan, node.MacroReference);
        }
    }
}