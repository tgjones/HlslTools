using System.Collections.Generic;

namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
{
    public sealed class NamespaceSyntax : SyntaxNode
    {
        public readonly SyntaxToken NamespaceKeyword;
        public readonly SyntaxToken Name;
        public readonly SyntaxToken OpenBraceToken;
        public readonly List<SyntaxNode> Declarations;
        public readonly SyntaxToken CloseBraceToken;
        public readonly SyntaxToken SemicolonToken;

        public NamespaceSyntax(SyntaxToken namespaceKeyword, SyntaxToken name, SyntaxToken openBraceToken, List<SyntaxNode> declarations, SyntaxToken closeBraceToken, SyntaxToken semicolonToken)
            : base(SyntaxKind.Namespace)
        {
            RegisterChildNode(out NamespaceKeyword, namespaceKeyword);
            RegisterChildNode(out Name, name);
            RegisterChildNode(out OpenBraceToken, openBraceToken);
            RegisterChildNodes(out Declarations, declarations);
            RegisterChildNode(out CloseBraceToken, closeBraceToken);
            RegisterChildNode(out SemicolonToken, semicolonToken);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitNamespace(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitNamespace(this);
        }
    }
}