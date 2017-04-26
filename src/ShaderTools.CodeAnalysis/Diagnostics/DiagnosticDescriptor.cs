namespace ShaderTools.CodeAnalysis.Diagnostics
{
    public sealed class DiagnosticDescriptor
    {
        public string Id { get; }
        public int Code { get; }
        public string MessageFormat { get; }
        public DiagnosticSeverity Severity { get; }

        public DiagnosticDescriptor(
            string id, 
            int code,
            string messageFormat, 
            DiagnosticSeverity severity)
        {
            Id = id;
            Code = code;
            MessageFormat = messageFormat;
            Severity = severity;
        }
    }
}
