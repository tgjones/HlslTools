namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundDiscardStatement : BoundStatement
    {
        public BoundDiscardStatement()
            : base(BoundNodeKind.DiscardStatement)
        {
        }
    }
}