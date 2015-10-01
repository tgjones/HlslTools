using HlslTools.Syntax;

namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundNoOpStatement : BoundStatement
    {
        public BoundNoOpStatement(EmptyStatementSyntax syntax)
            : base(BoundNodeKind.NoOpStatement, syntax)
        {
        }
    }
}