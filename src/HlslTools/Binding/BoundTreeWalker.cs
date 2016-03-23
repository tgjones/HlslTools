using System;
using HlslTools.Binding.BoundNodes;

namespace HlslTools.Binding
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