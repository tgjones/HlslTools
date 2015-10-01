using System.Collections.Immutable;
using HlslTools.Syntax;

namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundForStatement : BoundLoopStatement
    {
        public BoundForStatement(ForStatementSyntax syntax, BoundVariableDeclaration declaration, ImmutableArray<BoundExpression> initializers, BoundExpression condition, ImmutableArray<BoundExpression> incrementors, BoundStatement body)
            : base(BoundNodeKind.ForStatement, syntax)
        {
            Declaration = declaration;
            Initializers = initializers;
            Condition = condition;
            Incrementors = incrementors;
            Body = body;
        }

        public BoundVariableDeclaration Declaration { get; }
        public ImmutableArray<BoundExpression> Initializers { get; }
        public BoundExpression Condition { get; }
        public ImmutableArray<BoundExpression> Incrementors { get; }
        public BoundStatement Body { get; }
    }
}