namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundSamplerStateInitializer : BoundInitializer
    {
        public BoundSamplerStateInitializer()
            : base(BoundNodeKind.SamplerState)
        {
        }
    }
}