using HlslTools.Symbols;
using HlslTools.Syntax;

namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundMemberExpression : BoundExpression
    {
        public BoundMemberExpression(SyntaxNode syntax, BoundExpression objectReference, Symbol member)
            : base(BoundNodeKind.MemberExpression, syntax)
        {
            ObjectReference = objectReference;
            Member = member;
            Type = ((IMemberSymbol) member).AssociatedType;
        }

        public override TypeSymbol Type { get; }
        public BoundExpression ObjectReference { get; }
        public Symbol Member { get; }
    }
}