using ShaderTools.Core.Diagnostics;

namespace ShaderTools.Unity.Diagnostics
{
    internal static class DiagnosticFacts
    {
        public static DiagnosticSeverity GetSeverity(DiagnosticId id)
        {
            return DiagnosticSeverity.Error;
        }
    }
}