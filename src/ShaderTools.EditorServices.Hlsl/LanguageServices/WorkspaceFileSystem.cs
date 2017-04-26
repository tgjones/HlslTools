using System.IO;
using ShaderTools.CodeAnalysis.Text;
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
            // Is file open in workspace?
            var document = _workspace.GetDocument(new DocumentId(path));
            if (document != null)
            {
                text = document.SourceText;
                return true;
            }

            // TODO: Don't open directly; open through workspace, so that it is pretokenized and cached.
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
