namespace HlslTools.Diagnostics
{
    internal static class DiagnosticFacts
    {
        public static DiagnosticSeverity GetSeverity(DiagnosticId id)
        {
            switch (id)
            {
                case DiagnosticId.LoopControlVariableConflict:
                    return DiagnosticSeverity.Warning;
                default:
                    return DiagnosticSeverity.Error;
            }
        }
    }
}