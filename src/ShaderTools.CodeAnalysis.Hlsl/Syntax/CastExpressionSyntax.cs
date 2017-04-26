using System.Collections.Generic;

namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
{
    public class CastExpressionSyntax : ExpressionSyntax
    {
        public readonly SyntaxToken OpenParenToken;
        public readonly TypeSyntax Type;
        public readonly List<ArrayRankSpecifierSyntax> ArrayRankSpecifiers;
        public readonly SyntaxToken CloseParenToken;
        public readonly ExpressionSyntax Expression;

        public CastExpressionSyntax(SyntaxToken openParenToken, TypeSyntax type, List<ArrayRankSpecifierSyntax> arrayRankSpecifiers, SyntaxToken closeParenToken, ExpressionSyntax expression)
            : base(SyntaxKind.CastExpression)
        {
            RegisterChildNode(out OpenParenToken, openParenToken);
            RegisterChildNode(out Type, type);
            RegisterChildNodes(out ArrayRankSpecifiers, arrayRankSpecifiers);
            RegisterChildNode(out CloseParenToken, closeParenToken);
            RegisterChildNode(out Expression, expression);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitPrefixCastExpression(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitPrefixCastExpression(this);
        }
    }
}