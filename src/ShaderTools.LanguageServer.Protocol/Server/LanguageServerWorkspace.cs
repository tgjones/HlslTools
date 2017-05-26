using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ShaderTools.CodeAnalysis;
using ShaderTools.CodeAnalysis.Host.Mef;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.LanguageServer.Protocol.Server
{
    public sealed class LanguageServerWorkspace : Workspace
    {
        public LanguageServerWorkspace()
            : base(CreateHostServices())
        {
        }

        private static MefHostServices CreateHostServices()
        {
            var assemblies = MefHostServices.DefaultAssemblies
                .Union(new[] { typeof(LanguageServerWorkspace).GetTypeInfo().Assembly });

            return MefHostServices.Create(assemblies);
        }

        public Document OpenDocument(DocumentId documentId, SourceText sourceText, string languageName)
        {
            var document = CreateDocument(documentId, languageName, sourceText, sourceText.FilePath);
            OnDocumentOpened(document);
            return document;
        }

        public Document UpdateDocument(Document document, IEnumerable<TextChange> changes)
        {
            var newText = document.SourceText.WithChanges(changes);
            throw new System.NotImplementedException();
            //return OnDocumentTextChanged(document.Id, newText);
        }

        public void CloseDocument(DocumentId documentId)
        {
            OnDocumentClosed(documentId);
        }
    }
}
