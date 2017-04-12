using System.Collections.Generic;
using System.Linq;
using ShaderTools.Core.Diagnostics;
using ShaderTools.Core.Syntax;
using ShaderTools.Core.Text;
using ShaderTools.Unity.Syntax;
using ShaderTools.Core.Parser;

namespace ShaderTools.Unity.Diagnostics
{
    internal static class DiagnosticExtensions
    {
        public static void Report(this ICollection<Diagnostic> diagnostics, SourceRange sourceRange, DiagnosticId diagnosticId, params object[] args)
        {
            var diagnostic = Diagnostic.Create(ShaderLabMessageProvider.Instance, sourceRange, (int) diagnosticId, args);
            diagnostics.Add(diagnostic);
        }

        public static void Report(this ICollection<PretokenizedDiagnostic> diagnostics, int offset, int width, DiagnosticId diagnosticId, params object[] args)
        {
            var diagnostic = PretokenizedDiagnostic.Create(ShaderLabMessageProvider.Instance, offset, width, (int) diagnosticId, args);
            diagnostics.Add(diagnostic);
        }

        #region Lexer errors

        public static void ReportIllegalInputCharacter(this ICollection<PretokenizedDiagnostic> diagnostics, char character)
        {
            diagnostics.Report(0, 0, DiagnosticId.IllegalInputCharacter, character);
        }

        public static void ReportUnterminatedComment(this ICollection<PretokenizedDiagnostic> diagnostics)
        {
            diagnostics.Report(0, 0, DiagnosticId.UnterminatedComment);
        }

        public static void ReportUnterminatedString(this ICollection<PretokenizedDiagnostic> diagnostics)
        {
            diagnostics.Report(0, 0, DiagnosticId.UnterminatedString);
        }

        public static void ReportInvalidInteger(this ICollection<PretokenizedDiagnostic> diagnostics, string tokenText)
        {
            diagnostics.Report(0, 0, DiagnosticId.InvalidInteger, tokenText);
        }

        public static void ReportInvalidReal(this ICollection<PretokenizedDiagnostic> diagnostics, string tokenText)
        {
            diagnostics.Report(0, 0, DiagnosticId.InvalidReal, tokenText);
        }

        public static void ReportInvalidOctal(this ICollection<PretokenizedDiagnostic> diagnostics, string tokenText)
        {
            diagnostics.Report(0, 0, DiagnosticId.InvalidOctal, tokenText);
        }

        public static void ReportInvalidHex(this ICollection<PretokenizedDiagnostic> diagnostics, string tokenText)
        {
            diagnostics.Report(0, 0, DiagnosticId.InvalidHex, tokenText);
        }

        public static void ReportNumberTooLarge(this ICollection<PretokenizedDiagnostic> diagnostics, string tokenText)
        {
            diagnostics.Report(0, 0, DiagnosticId.NumberTooLarge, tokenText);
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