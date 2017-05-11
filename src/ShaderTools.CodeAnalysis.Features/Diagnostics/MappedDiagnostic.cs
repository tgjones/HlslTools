using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.Diagnostics
{
    internal sealed class MappedDiagnostic
    {
        public Diagnostic Diagnostic { get; }
        public SourceFileSpan FileSpan { get; }
        public DiagnosticSource Source { get; }

        public MappedDiagnostic(Diagnostic diagnostic, DiagnosticSource source, SourceFileSpan fileSpan)
        {
            Diagnostic = diagnostic;
            FileSpan = fileSpan;
            Source = source;
        }
    }
}