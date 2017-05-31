// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;

namespace ShaderTools.CodeAnalysis.Editor.CommandHandlers
{
    [Export]
    [ExportCommandHandler(PredefinedCommandHandlerNames.Completion, ContentTypeNames.ShaderToolsContentType)]
    [Order(After = PredefinedCommandHandlerNames.SignatureHelp,
        Before = PredefinedCommandHandlerNames.DocumentationComments)]
    internal sealed class CompletionCommandHandler : AbstractCompletionCommandHandler
    {
        [ImportingConstructor]
        public CompletionCommandHandler(IAsyncCompletionService completionService)
            : base(completionService)
        {
        }
    }
}
