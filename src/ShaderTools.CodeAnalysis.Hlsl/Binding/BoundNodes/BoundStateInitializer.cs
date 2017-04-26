namespace ShaderTools.CodeAnalysis.Hlsl.Binding.BoundNodes
{
    internal sealed class BoundStateInitializer : BoundInitializer
    {
        public BoundStateInitializer()
            : base(BoundNodeKind.StateInitializer)
        {
        }
    }
}