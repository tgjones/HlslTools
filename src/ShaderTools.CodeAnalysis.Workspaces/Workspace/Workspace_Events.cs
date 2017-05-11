using System;

namespace ShaderTools.CodeAnalysis
{
    public abstract partial class Workspace
    {
        public event EventHandler<DocumentEventArgs> DocumentOpened;
        public event EventHandler<DocumentEventArgs> DocumentClosed;

        public event EventHandler<DocumentEventArgs> DocumentChanged;
    }
}
