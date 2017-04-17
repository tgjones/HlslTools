using System.IO;
using ShaderTools.Core.Text;
using ShaderTools.EditorServices.Workspace;

namespace ShaderTools.EditorServices.Hlsl.LanguageServices
{
    internal sealed class WorkspaceFileSystem : IWorkspaceIncludeFileSystem
    {
        private readonly Workspace.Workspace _workspace;

        public WorkspaceFileSystem(Workspace.Workspace workspace)
        {
            _workspace = workspace;
        }

        public bool TryGetFile(string path, out SourceText text)
        {
            Document document;
            if (_workspace.TryGetDocument(path, out document))
            {
                text = document.SourceText;
                return true;
            }

            if (File.Exists(path))
            {
                text = SourceText.From(File.ReadAllText(path), path);
                return true;
            }

            text = null;
            return false;
        }
    }
}
