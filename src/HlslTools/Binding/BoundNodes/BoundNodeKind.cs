namespace HlslTools.Binding.BoundNodes
{
    internal enum BoundNodeKind
    {
        CompilationUnit,

        // Types
        StructType,

        // Expressions
        FunctionInvocationExpression,
        UnaryExpression,
        BinaryExpression,
        ConditionalExpression,
        ElementAccessExpression,
        LiteralExpression,
        LocalExpression,
        GlobalExpression,
        MemberExpression,

        // Statements
        Block,
        BreakStatement,
        ContinueStatement,
        DiscardStatement,
        ExpressionStatement,
        IfStatement,
        ForStatement,
        DoStatement,
        WhileStatement,
        ReturnStatement,
        SwitchStatement,
        SwitchSection,
        SwitchLabel,
        NoOpStatement,
        VariableDeclaration,
        MultipleVariableDeclarations
    }
}