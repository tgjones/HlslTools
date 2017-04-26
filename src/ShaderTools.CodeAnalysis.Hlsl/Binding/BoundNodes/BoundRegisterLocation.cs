namespace ShaderTools.CodeAnalysis.Hlsl.Binding.BoundNodes
{
    internal sealed class BoundRegisterLocation : BoundVariableQualifier
    {
        public BoundRegisterLocation()
            : base(BoundNodeKind.RegisterLocation)
        {

        }
    }
}