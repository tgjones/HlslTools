using ShaderTools.Core.Text;

namespace ShaderTools.EditorServices.Workspace.Hlsl
{
    public sealed class HlslWorkspace : Workspace
    {
        private readonly WorkspaceFileSystem _fileSystem;

        public HlslWorkspace()
            : base(null)
        {
            _fileSystem = new WorkspaceFileSystem(this);
        }

        protected override Document CreateDocument(SourceText sourceText, string clientFilePath)
        {
            return new HlslDocument(sourceText, clientFilePath, _fileSystem, this);
        }
    }
}
