using System.Collections.Immutable;
using HlslTools.Syntax;

namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundBlock : BoundStatement
    {
        public ImmutableArray<BoundStatement> Statements { get; set; }

        public BoundBlock(BlockSyntax syntax, ImmutableArray<BoundStatement> statements)
            : base(BoundNodeKind.Block, syntax)
        {
            Statements = statements;
        }
    }
}