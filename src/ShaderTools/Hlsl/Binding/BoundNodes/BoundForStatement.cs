namespace ShaderTools.Hlsl.Binding.BoundNodes
{
    internal sealed class BoundForStatement : BoundLoopStatement
    {
        public BoundForStatement(BoundMultipleVariableDeclarations declaration, BoundExpression initializer, BoundExpression condition, BoundExpression incrementor, BoundStatement body)
            : base(BoundNodeKind.ForStatement)
        {
            Declarations = declaration;
            Initializer = initializer;
            Condition = condition;
            Incrementor = incrementor;
            Body = body;
        }

        public BoundMultipleVariableDeclarations Declarations { get; }
        public BoundExpression Initializer { get; }
        public BoundExpression Condition { get; }
        public BoundExpression Incrementor { get; }
        public BoundStatement Body { get; }
    }
}