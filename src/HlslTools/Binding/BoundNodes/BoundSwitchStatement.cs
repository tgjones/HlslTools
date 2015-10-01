using System.Collections.Immutable;
using HlslTools.Syntax;

namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundSwitchStatement : BoundStatement
    {
        public BoundExpression Expression { get; set; }
        public ImmutableArray<BoundSwitchSection> Sections { get; set; }

        public BoundSwitchStatement(SwitchStatementSyntax syntax, BoundExpression expression, ImmutableArray<BoundSwitchSection> sections)
            : base(BoundNodeKind.SwitchStatement, syntax)
        {
            Expression = expression;
            Sections = sections;
        }
    }
}