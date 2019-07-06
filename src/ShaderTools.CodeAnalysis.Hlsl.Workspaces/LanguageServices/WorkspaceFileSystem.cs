using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.Hlsl.LanguageServices
{
    internal sealed class WorkspaceFileSystem : IWorkspaceIncludeFileSystem
    {
        private readonly Workspace _workspace;

        public WorkspaceFileSystem(Workspace workspace)
        {
            _workspace = workspace;
        }

        public bool TryGetFile(string path, out SourceText text)
        {
            // Is file open in workspace?
            var document = _workspace.CurrentDocuments
                .GetDocumentsWithFilePath(path)
                .FirstOrDefault();

            if (document != null)
            {
                text = document.SourceText;
                return true;
            }

            // TODO: Don't open directly; open through workspace, so that it is pretokenized and cached.
            if (File.Exists(path))
            {
                text = SourceText.From(File.ReadAllText(path));
                return true;
            }

            text = null;
            return false;
        }
    }
}
