using HlslTools.Syntax;

namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundDoStatement : BoundLoopStatement
    {
        public BoundDoStatement(DoStatementSyntax syntax, BoundExpression condition, BoundStatement body)
            : base(BoundNodeKind.DoStatement, syntax)
        {
            Condition = condition;
            Body = body;
        }

        public BoundExpression Condition { get; }
        public BoundStatement Body { get; }
    }
}