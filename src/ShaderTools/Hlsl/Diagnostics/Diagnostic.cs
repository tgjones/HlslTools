using System.Globalization;
using ShaderTools.Core.Diagnostics;
using ShaderTools.Core.Text;

namespace ShaderTools.Hlsl.Diagnostics
{
    public sealed class Diagnostic : DiagnosticBase
    {
        public DiagnosticId DiagnosticId { get; }

        public Diagnostic(TextSpan textSpan, DiagnosticId diagnosticId, string message)
            : base(textSpan, message, DiagnosticFacts.GetSeverity(diagnosticId))
        {
            DiagnosticId = diagnosticId;
        }

        public static Diagnostic Format(TextSpan textSpan, DiagnosticId diagnosticId, params object[] args)
        {
            var message = diagnosticId.GetMessage();
            var formattedMessage = (message != null)
                ? string.Format(CultureInfo.CurrentCulture, message, args)
                : $"Missing diagnostic message for {diagnosticId}";
            return new Diagnostic(textSpan, diagnosticId, formattedMessage);
        }
    }
}