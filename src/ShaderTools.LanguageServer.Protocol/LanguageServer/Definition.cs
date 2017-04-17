//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

using ShaderTools.LanguageServer.Protocol.MessageProtocol;

namespace ShaderTools.LanguageServer.Protocol.LanguageServer
{
    public class DefinitionRequest
    {
        public static readonly
            RequestType<TextDocumentPosition, Location[]> Type =
            RequestType<TextDocumentPosition, Location[]>.Create("textDocument/definition");
    }
}

