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

        public Document OpenDocument(DocumentId documentId, SourceText sourceText, string languageName)
        {
            var document = CreateDocument(documentId, languageName, sourceText, sourceText.FilePath);
            OnDocumentOpened(document);
            return document;
        }
    }
}
