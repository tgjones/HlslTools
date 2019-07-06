using System;

namespace ShaderTools.CodeAnalysis.Diagnostics
{
    internal sealed class DiagnosticsUpdatedEventArgs : EventArgs
    {
        public Document Document { get; }

        public DiagnosticsUpdatedEventArgs(Document document)
        {
            Document = document;
        }
    }
}