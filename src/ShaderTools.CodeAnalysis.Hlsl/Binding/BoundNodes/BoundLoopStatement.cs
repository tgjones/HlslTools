namespace ShaderTools.CodeAnalysis.Hlsl.Binding.BoundNodes
{
    internal abstract class BoundLoopStatement : BoundStatement
    {
        protected BoundLoopStatement(BoundNodeKind kind)
            : base(kind)
        {
        }
    }
}