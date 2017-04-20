using System;

namespace ShaderTools.EditorServices.Workspace
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
