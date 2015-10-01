using HlslTools.Syntax;

namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundDiscardStatement : BoundStatement
    {
        public BoundDiscardStatement(BreakStatementSyntax syntax)
            : base(BoundNodeKind.DiscardStatement, syntax)
        {
        }
    }
}