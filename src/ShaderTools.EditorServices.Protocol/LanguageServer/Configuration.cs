//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

using ShaderTools.EditorServices.Protocol.MessageProtocol;

namespace ShaderTools.EditorServices.Protocol.LanguageServer
{
    public class DidChangeConfigurationNotification<TConfig> 
    {
        public static readonly
            EventType<DidChangeConfigurationParams<TConfig>> Type =
            EventType<DidChangeConfigurationParams<TConfig>>.Create("workspace/didChangeConfiguration");
    }

    public class DidChangeConfigurationParams<TConfig>
    {
        public TConfig Settings { get; set; }
    }
}
