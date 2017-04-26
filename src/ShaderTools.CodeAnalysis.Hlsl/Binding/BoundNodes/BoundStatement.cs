namespace ShaderTools.CodeAnalysis.Hlsl.Binding.BoundNodes
{
    internal abstract class BoundStatement : BoundNode
    {
        protected BoundStatement(BoundNodeKind kind)
            : base(kind)
        {
        }
    }
}