using HlslTools.Symbols;
using HlslTools.Syntax;

namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundBadExpression : BoundExpression
    {
        public BoundBadExpression(SyntaxNode syntax)
            : base(BoundNodeKind.Bad)
        {
            Type = null;
            SyntaxNode = syntax;
        }

        public override TypeSymbol Type { get; }
        public SyntaxNode SyntaxNode { get; }
    }
}