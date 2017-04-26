using ShaderTools.CodeAnalysis.Syntax;

namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
{
    public class ArgumentListSyntax : SyntaxNode
    {
        public readonly SyntaxToken OpenParenToken;
        public readonly SeparatedSyntaxList<ExpressionSyntax> Arguments;
        public readonly SyntaxToken CloseParenToken;

        public ArgumentListSyntax(SyntaxToken openParenToken, SeparatedSyntaxList<ExpressionSyntax> arguments, SyntaxToken closeParenToken)
            : base(SyntaxKind.ArgumentList)
        {
            RegisterChildNode(out OpenParenToken, openParenToken);
            RegisterChildNodes(out Arguments, arguments);
            RegisterChildNode(out CloseParenToken, closeParenToken);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitArgumentList(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitArgumentList(this);
        }
    }
}