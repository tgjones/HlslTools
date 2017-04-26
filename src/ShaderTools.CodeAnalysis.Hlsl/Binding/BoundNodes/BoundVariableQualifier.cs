namespace ShaderTools.CodeAnalysis.Hlsl.Binding.BoundNodes
{
    internal abstract class BoundVariableQualifier : BoundNode
    {
        protected BoundVariableQualifier(BoundNodeKind kind)
            : base(kind)
        {
        }
    }
}