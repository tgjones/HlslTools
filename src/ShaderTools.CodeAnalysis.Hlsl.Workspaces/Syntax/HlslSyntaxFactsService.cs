using Microsoft.CodeAnalysis.Host.Mef;
using ShaderTools.CodeAnalysis.Syntax;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
{
    [ExportLanguageService(typeof(ISyntaxFactsService), LanguageNames.Hlsl)]
    internal sealed class HlslSyntaxFactsService : ISyntaxFactsService
    {
        public SourceFileSpan? GetFileSpanRoot(SyntaxNodeBase node)
        {
            return ((SyntaxNode) node).GetTextSpanRoot();
        }

        public string GetKindText(ushort kind)
        {
            return ((SyntaxKind) kind).ToString();
        }

        public bool IsBindableToken(ISyntaxToken rawToken)
        {
            var token = (SyntaxToken) rawToken;

            if (token.MacroReference != null)
            {
                return false;
            }

            if (token.IsWord() || token.Kind.IsLiteral() || token.Kind.IsOperator())
            {
                switch (token.Kind)
                {
                    case SyntaxKind.VoidKeyword:
                        return false;
                }

                return true;
            }

            return false;
        }

        public SyntaxNodeBase GetBindableParent(ISyntaxToken token)
        {
            var node = token.Parent;
            while (node != null)
            {
                var parent = node.Parent;

                // If this node is on the left side of a member access expression, don't ascend 
                // further or we'll end up binding to something else.
                var memberAccess = parent as FieldAccessExpressionSyntax;
                if (memberAccess != null)
                {
                    if (memberAccess.Expression == node)
                    {
                        break;
                    }
                }

                // If this node is on the left side of a member access expression, don't ascend 
                // further or we'll end up binding to something else.
                var methodInvocation = parent as MethodInvocationExpressionSyntax;
                if (methodInvocation != null)
                {
                    if (methodInvocation.Target == node)
                    {
                        break;
                    }
                }

                // If this node is on the left side of a qualified name, don't ascend 
                // further or we'll end up binding to something else.
                var qualifiedName = parent as QualifiedNameSyntax;
                if (qualifiedName != null)
                {
                    if (qualifiedName.Left == node)
                    {
                        break;
                    }
                }

                // If this node is the type of an object creation expression, return the
                // object creation expression.
                var objectCreation = parent as NumericConstructorInvocationExpressionSyntax;
                if (objectCreation != null)
                {
                    if (objectCreation.Type == node)
                    {
                        node = parent;
                        break;
                    }
                }

                var attribute = parent as AttributeSyntax;
                if (attribute != null)
                {
                    if (attribute.Name == node)
                    {
                        node = parent;
                        break;
                    }
                }

                // If this node is not parented by a name, we're done.
                var name = parent as NameSyntax;
                if (name == null)
                {
                    break;
                }

                node = parent;
            }

            return node;
        }

        public bool IsCaseSensitive => true;

        public bool IsIdentifierStartCharacter(char c)
        {
            return SyntaxFacts.IsIdentifierStartCharacter(c);
        }

        public bool IsIdentifierPartCharacter(char c)
        {
            return SyntaxFacts.IsIdentifierPartCharacter(c);
        }
    }
}
