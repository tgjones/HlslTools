namespace HlslTools.Binding.BoundNodes
{
    internal enum BoundNodeKind
    {
        CompilationUnit,

        // Types
        TypeDeclaration,
        StructType,
        ClassType,
        InterfaceType,

        // Expressions
        FunctionInvocationExpression,
        MethodInvocationExpression,
        NumericConstructorInvocationExpression,
        UnaryExpression,
        BinaryExpression,
        CastExpression,
        ConditionalExpression,
        ElementAccessExpression,
        LiteralExpression,
        StringLiteralExpression,
        MemberExpression,
        VariableExpression,
        MethodName,
        FunctionName,

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

        // Declarations
        VariableDeclaration,
        MultipleVariableDeclarations,
        Function,
        ConstantBuffer,

        // Other
        EqualsValue,
        Bad
    }
}