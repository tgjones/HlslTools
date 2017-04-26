namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
{
    public class BaseListSyntax : SyntaxNode
    {
        public readonly SyntaxToken ColonToken;
        public readonly IdentifierNameSyntax BaseType;

        public BaseListSyntax(SyntaxToken colonToken, IdentifierNameSyntax baseType)
            : base(SyntaxKind.BaseList)
        {
            RegisterChildNode(out ColonToken, colonToken);
            RegisterChildNode(out BaseType, baseType);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitBaseList(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitBaseList(this);
        }
    }
}