// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Text;
using ShaderTools.CodeAnalysis.Host;
using ShaderTools.CodeAnalysis.Host.Mef;

namespace ShaderTools.CodeAnalysis.Editor.Shared.Preview
{
    internal class PreviewWorkspace : Workspace
    {
        public PreviewWorkspace()
            : base(MefHostServices.DefaultHost)
        {
        }

        public PreviewWorkspace(HostServices hostServices)
            : base(hostServices)
        {
        }

        public Document OpenDocument(DocumentId documentId, SourceText sourceText, string languageName)
        {
            var document = CreateDocument(documentId, languageName, new Text.SourceFile(sourceText));
            OnDocumentOpened(document);
            return document;
        }

        public void UpdateDocument(DocumentId documentId, SourceText sourceText)
        {
            OnDocumentTextChanged(documentId, sourceText);
        }

        public void CloseDocument(DocumentId documentId)
        {
            OnDocumentClosed(documentId);
        }
    }
}
