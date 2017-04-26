namespace ShaderTools.CodeAnalysis.ShaderLab.Diagnostics
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
        TokenExpectedMultipleChoices,
        TokenUnexpected,

        InvalidExprTerm,

        ExpressionExpected
    }
}