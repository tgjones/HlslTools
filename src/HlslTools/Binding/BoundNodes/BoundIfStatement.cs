using HlslTools.Syntax;

namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundIfStatement : BoundStatement
    {
        public BoundExpression Condition { get; }
        public BoundStatement Consequence { get; }
        public BoundStatement AlternativeOpt { get; }

        public BoundIfStatement(IfStatementSyntax syntax, BoundExpression condition, BoundStatement consequence, BoundStatement alternativeOpt)
            : base(BoundNodeKind.IfStatement, syntax)
        {
            Condition = condition;
            Consequence = consequence;
            AlternativeOpt = alternativeOpt;
        }
    }
}