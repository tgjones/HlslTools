namespace HlslTools.Binding.BoundNodes
{
    internal abstract class BoundType : BoundNode
    {
        protected BoundType(BoundNodeKind kind)
            : base(kind)
        {
        }
    }
}