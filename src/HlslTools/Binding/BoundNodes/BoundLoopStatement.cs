using HlslTools.Syntax;

namespace HlslTools.Binding.BoundNodes
{
    internal abstract class BoundLoopStatement : BoundStatement
    {
        protected BoundLoopStatement(BoundNodeKind kind, SyntaxNode syntax)
            : base(kind, syntax)
        {
        }
    }
}