using HlslTools.Syntax;

namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundWhileStatement : BoundLoopStatement
    {
        public BoundWhileStatement(WhileStatementSyntax syntax, BoundExpression condition, BoundStatement body)
            : base(BoundNodeKind.WhileStatement, syntax)
        {
            Condition = condition;
            Body = body;
        }

        public BoundExpression Condition { get; }
        public BoundStatement Body { get; }
    }
}