using System.ComponentModel.Composition;
using HlslTools.Compilation;
using HlslTools.Syntax;
using HlslTools.VisualStudio.Util.SyntaxOutput;

namespace HlslTools.VisualStudio.IntelliSense.QuickInfo.QuickInfoModelProviders
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

                string kind;
                switch (node.Parent.Kind)
                {
                    case SyntaxKind.ForStatement:
                        kind = "local variable";
                        break;

                    case SyntaxKind.VariableDeclarationStatement:
                        switch (node.Parent.Parent.Kind)
                        {
                            case SyntaxKind.CompilationUnit:
                                kind = "global variable";
                                break;

                            case SyntaxKind.StructType:
                            case SyntaxKind.ClassType:
                            case SyntaxKind.ConstantBufferDeclaration:
                                kind = "field";
                                break;

                            case SyntaxKind.Annotations:
                                kind = "annotation";
                                break;

                            case SyntaxKind.Block:
                                kind = "local variable";
                                break;

                            default:
                                continue;
                        }
                        break;

                    default:
                        continue;
                }

                return new QuickInfoModel(semanticModel, declarator.Identifier.Span, $"({kind}) {node.GetDescription(declarator)}");
            }

            return null;
        }
    }
}