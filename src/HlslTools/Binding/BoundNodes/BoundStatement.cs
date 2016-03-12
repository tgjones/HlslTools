namespace HlslTools.Binding.BoundNodes
{
    internal abstract class BoundStatement : BoundNode
    {
        protected BoundStatement(BoundNodeKind kind)
            : base(kind)
        {
        }
    }
}