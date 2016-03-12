namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundBreakStatement : BoundStatement
    {
        public BoundBreakStatement()
            : base(BoundNodeKind.BreakStatement)
        {
        }
    }
}