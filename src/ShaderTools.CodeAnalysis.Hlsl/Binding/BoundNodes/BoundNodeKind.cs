namespace ShaderTools.CodeAnalysis.Hlsl.Binding.BoundNodes
{
    internal enum BoundNodeKind
    {
        CompilationUnit,
        Namespace,

        // Types
        Name,
        TypeDeclaration,
        StructType,
        InterfaceType,
        IntrinsicMatrixType,
        IntrinsicGenericMatrixType,
        IntrinsicVectorType,
        IntrinsicGenericVectorType,
        IntrinsicScalarType,
        IntrinsicObjectType,
        UnknownType,

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
        Typedef,
        TypeAlias,

        // Other
        EqualsValue,
        StateInitializer,
        StateArrayInitializer,
        SamplerState,
        Semantic,
        PackOffsetLocation,
        RegisterLocation,
        Attribute,
        ErrorExpression,
        Error
    }
}