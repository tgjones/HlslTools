namespace ShaderTools.CodeAnalysis.Hlsl.Binding.BoundNodes
{
    internal sealed class BoundPass : BoundNode
    {
        public BoundPass()
            : base(BoundNodeKind.Pass)
        {
        }
    }
}