namespace ShaderTools.CodeAnalysis.Hlsl.Binding.BoundNodes
{
    internal sealed class BoundBreakStatement : BoundStatement
    {
        public BoundBreakStatement()
            : base(BoundNodeKind.BreakStatement)
        {
        }
    }
}