using ShaderTools.CodeAnalysis.Hlsl.Binding.BoundNodes;

namespace ShaderTools.CodeAnalysis.Hlsl.Binding
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