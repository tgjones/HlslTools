using System.Collections.Generic;

namespace ShaderTools.CodeAnalysis.ShaderLab.Syntax
{
    public sealed class EnumNameExpressionSyntax : ExpressionSyntax
    {
        public readonly List<SyntaxToken> NameTokens;

        public EnumNameExpressionSyntax(List<SyntaxToken> nameTokens)
            : base(SyntaxKind.EnumNameExpression)
        {
            RegisterChildNodes(out NameTokens, nameTokens);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitEnumNameExpression(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitEnumNameExpression(this);
        }
    }
}