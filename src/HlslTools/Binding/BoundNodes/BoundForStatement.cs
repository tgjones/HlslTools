using System.Collections.Immutable;

namespace HlslTools.Binding.BoundNodes
{
    internal sealed class BoundForStatement : BoundLoopStatement
    {
        public BoundForStatement(BoundVariableDeclaration declaration, ImmutableArray<BoundExpression> initializers, BoundExpression condition, ImmutableArray<BoundExpression> incrementors, BoundStatement body)
            : base(BoundNodeKind.ForStatement)
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