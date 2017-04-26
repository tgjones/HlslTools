namespace ShaderTools.CodeAnalysis.Hlsl.Binding.BoundNodes
{
    internal sealed class BoundNoOpStatement : BoundStatement
    {
        public BoundNoOpStatement()
            : base(BoundNodeKind.NoOpStatement)
        {
        }
    }
}