using HlslTools.Syntax;

namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundBreakStatement : BoundStatement
    {
        public BoundBreakStatement(BreakStatementSyntax syntax)
            : base(BoundNodeKind.BreakStatement, syntax)
        {
        }
    }
}