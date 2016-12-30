using System.Collections.Generic;
using System.Linq;
using ShaderTools.Core.Diagnostics;
using ShaderTools.Core.Syntax;
using ShaderTools.Core.Text;
using ShaderTools.Unity.Syntax;

namespace ShaderTools.Unity.Diagnostics
{
    internal static class DiagnosticExtensions
    {
        public static void Report(this ICollection<Diagnostic> diagnostics, SourceRange sourceRange, DiagnosticId diagnosticId, params object[] args)
        {
            var diagnostic = Diagnostic.Create(ShaderLabMessageProvider.Instance, sourceRange, (int) diagnosticId, args);
            diagnostics.Add(diagnostic);
        }

        #region Lexer errors

        public static void ReportIllegalInputCharacter(this ICollection<Diagnostic> diagnostics, SourceRange sourceRange, char character)
        {
            diagnostics.Report(sourceRange, DiagnosticId.IllegalInputCharacter, character);
        }

        public static void ReportUnterminatedComment(this ICollection<Diagnostic> diagnostics, SourceRange sourceRange)
        {
            diagnostics.Report(sourceRange, DiagnosticId.UnterminatedComment);
        }

        public static void ReportUnterminatedString(this ICollection<Diagnostic> diagnostics, SourceRange sourceRange)
        {
            diagnostics.Report(sourceRange, DiagnosticId.UnterminatedString);
        }

        public static void ReportInvalidInteger(this ICollection<Diagnostic> diagnostics, SourceRange sourceRange, string tokenText)
        {
            diagnostics.Report(sourceRange, DiagnosticId.InvalidInteger, tokenText);
        }

        public static void ReportInvalidReal(this ICollection<Diagnostic> diagnostics, SourceRange sourceRange, string tokenText)
        {
            diagnostics.Report(sourceRange, DiagnosticId.InvalidReal, tokenText);
        }

        public static void ReportInvalidOctal(this ICollection<Diagnostic> diagnostics, SourceRange sourceRange, string tokenText)
        {
            diagnostics.Report(sourceRange, DiagnosticId.InvalidOctal, tokenText);
        }

        public static void ReportInvalidHex(this ICollection<Diagnostic> diagnostics, SourceRange sourceRange, string tokenText)
        {
            diagnostics.Report(sourceRange, DiagnosticId.InvalidHex, tokenText);
        }

        public static void ReportNumberTooLarge(this ICollection<Diagnostic> diagnostics, SourceRange sourceRange, string tokenText)
        {
            diagnostics.Report(sourceRange, DiagnosticId.NumberTooLarge, tokenText);
        }

        #endregion

        #region Parser errors

        public static void ReportTokenExpected(this ICollection<Diagnostic> diagnostics, SourceRange sourceRange, SyntaxToken actual, SyntaxKind expected)
        {
            var actualText = actual.GetDisplayText();
            var expectedText = expected.GetDisplayText();
            diagnostics.Report(sourceRange, DiagnosticId.TokenExpected, actualText, expectedText);
        }

        public static void ReportTokenExpectedMultipleChoices(this ICollection<Diagnostic> diagnostics, SourceRange sourceRange, SyntaxToken actual, IEnumerable<SyntaxKind> expected)
        {
            var actualText = actual.GetDisplayText();
            var expectedText = string.Join(", ", expected.Select(x => $"'{x.GetDisplayText()}'"));
            diagnostics.Report(sourceRange, DiagnosticId.TokenExpectedMultipleChoices, actualText, expectedText);
        }

        public static void ReportTokenUnexpected(this ICollection<Diagnostic> diagnostics, SourceRange sourceRange, SyntaxToken actual)
        {
            var actualText = actual.GetDisplayText();
            diagnostics.Report(sourceRange, DiagnosticId.TokenUnexpected, actualText);
        }

        #endregion

        #region Semantic errors

        

        #endregion
    }
}