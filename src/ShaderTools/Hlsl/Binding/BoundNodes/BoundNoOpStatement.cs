namespace ShaderTools.Hlsl.Binding.BoundNodes
{
    internal sealed class BoundNoOpStatement : BoundStatement
    {
        public BoundNoOpStatement()
            : base(BoundNodeKind.NoOpStatement)
        {
        }
    }
}