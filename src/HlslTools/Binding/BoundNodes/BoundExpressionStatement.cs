using HlslTools.Syntax;

namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundExpressionStatement : BoundStatement
    {
        public BoundExpressionStatement(ExpressionStatementSyntax syntax, BoundExpression expression)
            : base(BoundNodeKind.ExpressionStatement, syntax)
        {
            Expression = expression;
        }

        public BoundExpression Expression { get; set; }
    }
}