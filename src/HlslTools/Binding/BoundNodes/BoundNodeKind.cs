namespace HlslTools.Binding.BoundNodes
{
    internal enum BoundNodeKind
    {
        CompilationUnit,
        Namespace,

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
        ConditionalExpression,
        ElementAccessExpression,
        LiteralExpression,
        StringLiteralExpression,
        MemberExpression,
        VariableExpression,
        MethodName,
        FunctionName,
        ConversionExpression,
        CompoundExpression,
        ArrayInitializerExpression,

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
        Technique,
        Pass,

        // Other
        EqualsValue,
        StateInitializer,
        StateArrayInitializer,
        SamplerState,
        Error
    }
}