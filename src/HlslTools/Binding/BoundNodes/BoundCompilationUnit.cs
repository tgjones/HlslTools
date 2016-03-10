using System.Collections.Immutable;
using HlslTools.Syntax;

namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundCompilationUnit : BoundNode
    {
        public ImmutableArray<BoundNode> Declarations { get; }

        public BoundCompilationUnit(CompilationUnitSyntax syntax, ImmutableArray<BoundNode> declarations)
            : base(BoundNodeKind.CompilationUnit, syntax)
        {
            Declarations = declarations;
        }
    }
}