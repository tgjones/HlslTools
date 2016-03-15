namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundTypeDeclaration : BoundNode
    {
        public BoundNode BoundType { get; }

        public BoundTypeDeclaration(BoundNode boundType)
            : base(BoundNodeKind.TypeDeclaration)
        {
            BoundType = boundType;
        }
    }
}