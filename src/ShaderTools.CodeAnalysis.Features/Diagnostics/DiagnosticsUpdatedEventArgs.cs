using System;

namespace ShaderTools.CodeAnalysis.Diagnostics
{
    internal sealed class DiagnosticsUpdatedEventArgs : EventArgs
    {
        public DocumentId DocumentId { get; }

        public DiagnosticsUpdatedEventArgs(DocumentId documentId)
        {
            DocumentId = documentId;
        }
    }
}