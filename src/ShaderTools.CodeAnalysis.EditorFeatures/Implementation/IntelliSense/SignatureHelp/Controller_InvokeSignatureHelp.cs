// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.VisualStudio.Commanding;
using Microsoft.VisualStudio.Text.Editor.Commanding.Commands;
using ShaderTools.CodeAnalysis.Editor.Commands;
using ShaderTools.CodeAnalysis.SignatureHelp;

namespace ShaderTools.CodeAnalysis.Editor.Implementation.IntelliSense.SignatureHelp
{
    internal partial class Controller
    {
        CommandState IChainedCommandHandler<InvokeSignatureHelpCommandArgs>.GetCommandState(InvokeSignatureHelpCommandArgs args, Func<CommandState> nextHandler)
        {
            AssertIsForeground();
            return CommandState.Available;
        }

        void IChainedCommandHandler<InvokeSignatureHelpCommandArgs>.ExecuteCommand(InvokeSignatureHelpCommandArgs args, Action nextHandler, CommandExecutionContext context)
        {
            AssertIsForeground();
            DismissSessionIfActive();

            var providers = GetProviders();
            if (providers == null)
            {
                return;
            }

            this.StartSession(providers, new SignatureHelpTriggerInfo(SignatureHelpTriggerReason.InvokeSignatureHelpCommand));
        }
    }
}
