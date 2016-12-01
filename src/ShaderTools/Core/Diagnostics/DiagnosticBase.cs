using System.Globalization;
using ShaderTools.Core.Diagnostics;
using ShaderTools.Core.Text;

namespace ShaderTools.Core.Diagnostics
{
    public abstract class DiagnosticBase
    {
        public TextSpan Span { get; }
        public string Message { get; }
        public DiagnosticSeverity Severity { get; }

        protected DiagnosticBase(TextSpan textSpan, string message, DiagnosticSeverity severity)
        {
            Span = textSpan;
            Message = message;
            Severity = severity;
        }

        public override string ToString()
        {
            return $"{Span} {Message}";
        }
    }
}