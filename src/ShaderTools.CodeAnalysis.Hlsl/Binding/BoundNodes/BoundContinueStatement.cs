namespace ShaderTools.CodeAnalysis.Hlsl.Binding.BoundNodes
{
    internal sealed class BoundContinueStatement : BoundStatement
    {
        public BoundContinueStatement()
            : base(BoundNodeKind.ContinueStatement)
        {
        }
    }
}