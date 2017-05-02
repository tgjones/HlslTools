using System;

namespace ShaderTools.CodeAnalysis
{
    public sealed class DocumentEventArgs : EventArgs
    {
        public Document Document { get; }

        public DocumentEventArgs(Document document)
        {
            Document = document;
        }
    }
}
