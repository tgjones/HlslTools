using System.ComponentModel.Composition;
using ShaderTools.Core.Syntax;
using ShaderTools.Hlsl.Compilation;
using ShaderTools.Hlsl.Syntax;

namespace ShaderTools.Editor.VisualStudio.Hlsl.IntelliSense.QuickInfo.QuickInfoModelProviders
{
    [Export(typeof(IQuickInfoModelProvider))]
    internal sealed class VariableDeclarationQuickInfoModelProvider : QuickInfoModelProvider<VariableDeclarationSyntax>
    {
        protected override QuickInfoModel CreateModel(SemanticModel semanticModel, SourceLocation position, VariableDeclarationSyntax node)
        {
            foreach (var declarator in node.Variables)
            {
                if (!declarator.Identifier.SourceRange.ContainsOrTouches(position))
                    continue;

                if (!declarator.Identifier.Span.IsInRootFile)
                    continue;

                if (declarator.Identifier.MacroReference != null)
                    continue;

                var symbol = semanticModel.GetDeclaredSymbol(declarator);
                if (symbol == null)
                    break;

                return QuickInfoModel.ForSymbol(semanticModel, declarator.Identifier.Span, symbol);
            }

            return null;
        }
    }
}