using HlslTools.Symbols;
using HlslTools.Syntax;

namespace HlslTools.Binding.BoundNodes
{
    internal abstract class BoundExpression : BoundNode
    {
        public abstract TypeSymbol Type { get; }

        protected BoundExpression(BoundNodeKind kind, SyntaxNode syntax)
            : base(kind, syntax)
        {
        }
    }
}