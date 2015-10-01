using HlslTools.Syntax;

namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundReturnStatement : BoundStatement
    {
        public BoundReturnStatement(ReturnStatementSyntax syntax, BoundExpression expressionOpt)
            : base(BoundNodeKind.ReturnStatement, syntax)
        {
            ExpressionOpt = expressionOpt;
        }

        public BoundExpression ExpressionOpt { get; }
    }
}