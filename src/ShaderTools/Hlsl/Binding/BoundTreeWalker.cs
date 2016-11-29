using ShaderTools.Hlsl.Binding.BoundNodes;

namespace ShaderTools.Hlsl.Binding
{
    internal abstract partial class BoundTreeWalker
    {
        public virtual void VisitCompilationUnit(BoundCompilationUnit node)
        {
            foreach (var declaration in node.Declarations)
                VisitTopLevelDeclaration(declaration);
        }
    }
}