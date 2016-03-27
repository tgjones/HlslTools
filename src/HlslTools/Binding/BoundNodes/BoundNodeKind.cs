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
        AssignmentExpression,
        ConditionalExpression,
        ElementAccessExpression,
        LiteralExpression,
        StringLiteralExpression,
        FieldExpression,
        VariableExpression,
        MethodName,
        FunctionName,
        ConversionExpression,
        CompoundExpression,
        ArrayInitializerExpression,
        CompileExpression,

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
        FunctionDeclaration,
        FunctionDefinition,
        ConstantBuffer,
        Technique,
        Pass,

        // Other
        EqualsValue,
        StateInitializer,
        StateArrayInitializer,
        SamplerState,
        ErrorExpression,
        Error
    }
}