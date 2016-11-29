namespace ShaderTools.Hlsl.Binding.BoundNodes
{
    internal sealed class BoundStateInitializer : BoundInitializer
    {
        public BoundStateInitializer()
            : base(BoundNodeKind.StateInitializer)
        {
        }
    }
}