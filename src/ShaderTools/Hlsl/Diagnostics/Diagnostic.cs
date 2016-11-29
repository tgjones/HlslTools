using System.Globalization;
using ShaderTools.Core.Text;
using ShaderTools.Hlsl.Text;

namespace ShaderTools.Hlsl.Diagnostics
{
    public sealed class Diagnostic
    {
        public TextSpan Span { get; }
        public DiagnosticId DiagnosticId { get; }
        public string Message { get; }
        public DiagnosticSeverity Severity { get; }

        public Diagnostic(TextSpan textSpan, DiagnosticId diagnosticId, string message)
        {
            Span = textSpan;
            Message = message;
            DiagnosticId = diagnosticId;
            Severity = DiagnosticFacts.GetSeverity(diagnosticId);
        }

        public static Diagnostic Format(TextSpan textSpan, DiagnosticId diagnosticId, params object[] args)
        {
            var message = diagnosticId.GetMessage();
            var formattedMessage = (message != null)
                ? string.Format(CultureInfo.CurrentCulture, message, args)
                : $"Missing diagnostic message for {diagnosticId}";
            return new Diagnostic(textSpan, diagnosticId, formattedMessage);
        }

        public override string ToString()
        {
            return $"{Span} {Message}";
        }
    }
}