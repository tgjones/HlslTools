namespace ShaderTools.Hlsl.Binding.BoundNodes
{
    internal abstract class BoundInitializer : BoundNode
    {
        protected BoundInitializer(BoundNodeKind kind)
            : base(kind)
        {
        }
    }
}