using HlslTools.Syntax;

namespace HlslTools.Binding.BoundNodes
{
    internal abstract class BoundStatement : BoundNode
    {
        protected BoundStatement(BoundNodeKind kind, SyntaxNode syntax)
            : base(kind, syntax)
        {
        }
    }
}