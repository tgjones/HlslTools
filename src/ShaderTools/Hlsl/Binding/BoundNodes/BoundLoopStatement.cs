namespace ShaderTools.Hlsl.Binding.BoundNodes
{
    internal abstract class BoundLoopStatement : BoundStatement
    {
        protected BoundLoopStatement(BoundNodeKind kind)
            : base(kind)
        {
        }
    }
}