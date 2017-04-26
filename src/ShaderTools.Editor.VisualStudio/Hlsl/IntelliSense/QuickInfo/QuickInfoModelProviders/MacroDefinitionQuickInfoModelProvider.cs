using System.ComponentModel.Composition;
using ShaderTools.CodeAnalysis.Hlsl.Compilation;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.Editor.VisualStudio.Hlsl.IntelliSense.QuickInfo.QuickInfoModelProviders
{
    [Export(typeof(IQuickInfoModelProvider))]
    internal sealed class MacroDefinitionQuickInfoModelProvider : QuickInfoModelProvider<DefineDirectiveTriviaSyntax>
    {
        protected override QuickInfoModel CreateModel(SemanticModel semanticModel, SourceLocation position, DefineDirectiveTriviaSyntax node)
        {
            if (!node.MacroName.SourceRange.ContainsOrTouches(position))
                return null;

            if (!node.MacroName.Span.IsInRootFile)
                return null;

            return QuickInfoModel.ForMacroDefinition(semanticModel, node.MacroName.Span, node);
        }
    }
}