// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.VisualStudio.Commanding;
using Microsoft.VisualStudio.Text.Editor.Commanding.Commands;
using ShaderTools.CodeAnalysis.Completion;

namespace ShaderTools.CodeAnalysis.Editor.Implementation.IntelliSense.Completion
{
    internal partial class Controller
    {
        CommandState IChainedCommandHandler<InvokeCompletionListCommandArgs>.GetCommandState(InvokeCompletionListCommandArgs args, Func<CommandState> nextHandler)
        {
            AssertIsForeground();
            return CommandState.Available;
        }

        void IChainedCommandHandler<InvokeCompletionListCommandArgs>.ExecuteCommand(InvokeCompletionListCommandArgs args, Action nextHandler, CommandExecutionContext context)
        {
            AssertIsForeground();

            // TODO(cyrusn): Should we ever call into nextHandler in this method?

            // First, always dismiss whatever session we might have already had.  We no longer need
            // it.
            DismissSessionIfActive();

            // Next create the session that represents that we now have a potential completion list.
            // Then tell it to start computing.
            var completionService = this.GetCompletionService();
            if (completionService == null)
            {
                return;
            }

            var trigger = CompletionTrigger.Invoke;
            StartNewModelComputation(completionService, trigger);
        }
    }
}