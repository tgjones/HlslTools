//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

using ShaderTools.LanguageServer.Protocol.MessageProtocol;

namespace ShaderTools.LanguageServer.Protocol.LanguageServer
{
    public class ReferencesRequest
    {
        public static readonly
            RequestType<ReferencesParams, Location[], object, TextDocumentRegistrationOptions> Type =
                RequestType<ReferencesParams, Location[], object, TextDocumentRegistrationOptions>.Create("textDocument/references");
    }

    public class ReferencesParams : TextDocumentPositionParams
    {
        public ReferencesContext Context { get; set; }
    }

    public class ReferencesContext
    {
        public bool IncludeDeclaration { get; set; }
    }
}

