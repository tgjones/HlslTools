//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

using ShaderTools.LanguageServer.Protocol.MessageProtocol;

namespace ShaderTools.LanguageServer.Protocol.LanguageServer
{
    public enum DocumentHighlightKind
    {
        Text = 1,
        Read = 2,
        Write = 3
    }

    public class DocumentHighlight
    {
        public Range Range { get; set; }

        public DocumentHighlightKind Kind { get; set; }
    }

    public class DocumentHighlightRequest
    {
        public static readonly
            RequestType<TextDocumentPositionParams, DocumentHighlight[], object, TextDocumentRegistrationOptions> Type =
                RequestType<TextDocumentPositionParams, DocumentHighlight[], object, TextDocumentRegistrationOptions>.Create("textDocument/documentHighlight");
    }
}

