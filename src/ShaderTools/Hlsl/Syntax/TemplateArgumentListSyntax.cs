using ShaderTools.Core.Syntax;

namespace ShaderTools.Hlsl.Syntax
{
    public class TemplateArgumentListSyntax : SyntaxNode
    {
        public readonly SyntaxToken LessThanToken;
        public readonly SeparatedSyntaxList<ExpressionSyntax> Arguments;
        public readonly SyntaxToken GreaterThanToken;

        public TemplateArgumentListSyntax(SyntaxToken lessThanToken, SeparatedSyntaxList<ExpressionSyntax> arguments, SyntaxToken greaterThanToken)
            : base(SyntaxKind.TemplateArgumentList)
        {
            RegisterChildNode(out LessThanToken, lessThanToken);
            RegisterChildNodes(out Arguments, arguments);
            RegisterChildNode(out GreaterThanToken, greaterThanToken);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitTemplateArgumentList(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitTemplateArgumentList(this);
        }
    }
}