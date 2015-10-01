using System.Collections.Generic;
using HlslTools.Properties;
using HlslTools.Syntax;
using HlslTools.Text;

namespace HlslTools.Diagnostics
{
    internal static class DiagnosticExtensions
    {
        public static string GetMessage(this DiagnosticId diagnosticId)
        {
            return Resources.ResourceManager.GetString(diagnosticId.ToString());
        }

        public static void Report(this ICollection<Diagnostic> diagnostics, TextSpan textSpan, DiagnosticId diagnosticId, params object[] args)
        {
            var diagnostic = Diagnostic.Format(textSpan, diagnosticId, args);
            diagnostics.Add(diagnostic);
        }

        #region Lexer Errors

        public static void ReportIllegalInputCharacter(this ICollection<Diagnostic> diagnostics, TextSpan textSpan, char character)
        {
            diagnostics.Report(textSpan, DiagnosticId.IllegalInputCharacter, character);
        }

        public static void ReportUnterminatedComment(this ICollection<Diagnostic> diagnostics, TextSpan textSpan)
        {
            diagnostics.Report(textSpan, DiagnosticId.UnterminatedComment);
        }

        public static void ReportUnterminatedString(this ICollection<Diagnostic> diagnostics, TextSpan textSpan)
        {
            diagnostics.Report(textSpan, DiagnosticId.UnterminatedString);
        }

        public static void ReportInvalidInteger(this ICollection<Diagnostic> diagnostics, TextSpan textSpan, string tokenText)
        {
            diagnostics.Report(textSpan, DiagnosticId.InvalidInteger, tokenText);
        }

        public static void ReportInvalidReal(this ICollection<Diagnostic> diagnostics, TextSpan textSpan, string tokenText)
        {
            diagnostics.Report(textSpan, DiagnosticId.InvalidReal, tokenText);
        }

        public static void ReportInvalidOctal(this ICollection<Diagnostic> diagnostics, TextSpan textSpan, string tokenText)
        {
            diagnostics.Report(textSpan, DiagnosticId.InvalidOctal, tokenText);
        }

        public static void ReportInvalidHex(this ICollection<Diagnostic> diagnostics, TextSpan textSpan, string tokenText)
        {
            diagnostics.Report(textSpan, DiagnosticId.InvalidHex, tokenText);
        }

        public static void ReportNumberTooLarge(this ICollection<Diagnostic> diagnostics, TextSpan textSpan, string tokenText)
        {
            diagnostics.Report(textSpan, DiagnosticId.NumberTooLarge, tokenText);
        }

        #endregion

        #region Parser errors

        public static void ReportTokenExpected(this ICollection<Diagnostic> diagnostics, TextSpan span, SyntaxToken actual, SyntaxKind expected)
        {
            var actualText = actual.GetDisplayText();
            var expectedText = expected.GetDisplayText();
            diagnostics.Report(span, DiagnosticId.TokenExpected, actualText, expectedText);
        }

        public static void ReportTokenUnexpected(this ICollection<Diagnostic> diagnostics, TextSpan span, SyntaxToken actual)
        {
            var actualText = actual.GetDisplayText();
            diagnostics.Report(span, DiagnosticId.TokenUnexpected, actualText);
        }

        public static void ReportNoVoidHere(this ICollection<Diagnostic> diagnostics, TextSpan textSpan)
        {
            diagnostics.Report(textSpan, DiagnosticId.NoVoidHere);
        }

        public static void ReportNoVoidParameter(this ICollection<Diagnostic> diagnostics, TextSpan textSpan)
        {
            diagnostics.Report(textSpan, DiagnosticId.NoVoidParameter);
        }

        #endregion
    }
}