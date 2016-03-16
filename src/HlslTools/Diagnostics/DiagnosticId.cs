namespace HlslTools.Diagnostics
{
    public enum DiagnosticId
    {
        IllegalInputCharacter,
        UnterminatedComment,
        UnterminatedString,

        InvalidInteger,
        InvalidReal,
        InvalidOctal,
        InvalidHex,
        NumberTooLarge,
        TokenExpected,
        TokenUnexpected,

        EndIfDirectiveExpected,
        UnexpectedDirective,
        DirectiveExpected,
        AlreadyDefined,
        BadDirectivePlacement,
        EndOfPreprocessorLineExpected,
        MissingPreprocessorFile,
        IncludeNotFound,
        NotEnoughMacroParameters,
        UnexpectedAttribute,

        InvalidExprTerm,

        NoVoidHere,
        NoVoidParameter,
        ExpressionExpected,
        TypeExpected,

        BadEmbeddedStatement,
        ConstantExpected,

        UndeclaredType,
        UndeclaredFunction,
        UndeclaredVariable,
        AmbiguousInvocation,
        AmbiguousReference,
        AmbiguousType,
        CannotConvert,
        InvocationRequiresParenthesis
    }
}