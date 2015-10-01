using HlslTools.Symbols;
using HlslTools.Syntax;

namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundMemberExpression : BoundExpression
    {
        public BoundMemberExpression(SyntaxNode syntax, BoundExpression objectReference, MemberSymbol member)
            : base(BoundNodeKind.MemberExpression, syntax)
        {
            ObjectReference = objectReference;
            Member = member;
            Type = member.AssociatedType;
        }

        public override TypeSymbol Type { get; }
        public BoundExpression ObjectReference { get; }
        public MemberSymbol Member { get; }
    }
}