using HlslTools.Symbols;

namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundMemberExpression : BoundExpression
    {
        public BoundMemberExpression(BoundExpression objectReference, Symbol member)
            : base(BoundNodeKind.MemberExpression)
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