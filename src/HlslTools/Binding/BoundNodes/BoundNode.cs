using HlslTools.Syntax;

namespace HlslTools.Binding.BoundNodes
{
    internal abstract class BoundNode
    {
        public BoundNodeKind Kind { get; }
        public SyntaxNode Syntax { get; }

        protected BoundNode(BoundNodeKind kind, SyntaxNode syntax)
        {
            Kind = kind;
            Syntax = syntax;
        }
    }
}