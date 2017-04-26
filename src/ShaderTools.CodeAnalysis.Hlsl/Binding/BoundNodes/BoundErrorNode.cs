namespace ShaderTools.CodeAnalysis.Hlsl.Binding.BoundNodes
{
    internal sealed class BoundErrorNode : BoundNode
    {
        public BoundErrorNode()
            : base(BoundNodeKind.Error)
        {
            
        }
    }
}