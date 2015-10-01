using System.Globalization;
using HlslTools.Text;

namespace HlslTools.Diagnostics
{
    public sealed class Diagnostic
    {
        public TextSpan Span { get; }
        public DiagnosticId DiagnosticId { get; }
        public string Message { get; }

        public Diagnostic(TextSpan textSpan, DiagnosticId diagnosticId, string message)
        {
            Span = textSpan;
            Message = message;
            DiagnosticId = diagnosticId;
        }

        public static Diagnostic Format(TextSpan textSpan, DiagnosticId diagnosticId, params object[] args)
        {
            var message = diagnosticId.GetMessage();
            var formattedMessage = string.Format(CultureInfo.CurrentCulture, message, args);
            return new Diagnostic(textSpan, diagnosticId, formattedMessage);
        }

        public override string ToString()
        {
            return $"{Span} {Message}";
        }
    }
}