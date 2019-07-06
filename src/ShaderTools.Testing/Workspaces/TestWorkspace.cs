using ShaderTools.CodeAnalysis;
using ShaderTools.CodeAnalysis.Host.Mef;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.Testing.Workspaces
{
    public class TestWorkspace : Workspace
    {
        public TestWorkspace() 
            : base(MefHostServices.Create(MefHostServices.DefaultAssemblies))
        {
        }

        public Document OpenDocument(DocumentId documentId, SourceFile file, string languageName)
        {
            var document = CreateDocument(documentId, languageName, file);
            OnDocumentOpened(document);
            return document;
        }
    }
}
