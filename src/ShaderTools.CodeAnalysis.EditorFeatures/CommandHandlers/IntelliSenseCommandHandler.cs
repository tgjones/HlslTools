// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Commanding;
using Microsoft.VisualStudio.Utilities;

namespace ShaderTools.CodeAnalysis.Editor.CommandHandlers
{
    [Export(typeof(ICommandHandler))]
    [ContentType(ContentTypeNames.ShaderToolsContentType)]
    [Name(nameof(IntelliSenseCommandHandler))]
    internal sealed class IntelliSenseCommandHandler : AbstractIntelliSenseCommandHandler
    {
        [ImportingConstructor]
        public IntelliSenseCommandHandler(
            CompletionCommandHandler completionCommandHandler,
            SignatureHelpCommandHandler signatureHelpCommandHandler)
            : base(completionCommandHandler,
                signatureHelpCommandHandler)
        {
        }
    }
}
