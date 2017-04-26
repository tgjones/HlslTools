using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.Editor.VisualStudio.Hlsl.Util.ContextQuery
{
    internal static class SyntaxTreeExtensions
    {
        public static bool IsPreprocessorKeywordContext(this SyntaxTree syntaxTree, SourceLocation position, SyntaxToken preProcessorTokenOnLeftOfPosition)
        {
            // cases:
            //  #|
            //  #d|
            //  # |
            //  # d|

            // note: comments are not allowed between the # and item.
            var token = preProcessorTokenOnLeftOfPosition;
            token = token.GetPreviousTokenIfTouchingWord(position);

            if (token.IsKind(SyntaxKind.HashToken))
                return true;

            return false;
        }

        public static bool IsTypeDeclarationContext(this SyntaxTree syntaxTree, SyntaxToken token)
        {
            if (token.Kind == SyntaxKind.None)
                return true;

            if (token.IsKind(SyntaxKind.OpenBraceToken))
            {
                if (token.Parent.IsKind(SyntaxKind.ClassType, SyntaxKind.StructType))
                    return true;
                if (token.Parent.IsKind(SyntaxKind.Namespace))
                    return true;
            }

            // class C {
            //   int i;
            //   |
            if (token.IsKind(SyntaxKind.SemiToken))
            {
                if (token.Parent.GetParent() is TypeDefinitionSyntax || token.Parent.GetParent() is CompilationUnitSyntax)
                {
                    return true;
                }
            }

            // class C {}
            // |

            // namespace N {}
            // |

            // class C {
            //    void Foo() {
            //    }
            //    |
            if (token.IsKind(SyntaxKind.CloseBraceToken))
            {
                if (token.Parent is TypeDefinitionSyntax)
                    return true;
                if (token.Parent.IsKind(SyntaxKind.Namespace))
                    return true;
                if (token.Parent.IsKind(SyntaxKind.Block) &&
                    token.Parent.GetParent().GetParent() is TypeDefinitionSyntax)
                {
                    return true;
                }
            }

            return false;
        }
    }
}