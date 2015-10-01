using HlslTools.Syntax;

namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundContinueStatement : BoundStatement
    {
        public BoundContinueStatement(ContinueStatementSyntax syntax)
            : base(BoundNodeKind.ContinueStatement, syntax)
        {
        }
    }
}