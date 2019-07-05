// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.VisualStudio.Commanding;
using Microsoft.VisualStudio.Text.Editor.Commanding.Commands;

namespace ShaderTools.CodeAnalysis.Editor.Implementation.IntelliSense.Completion
{
    internal partial class Controller
    {
        // Cut and Paste should always dismiss completion

        CommandState IChainedCommandHandler<CutCommandArgs>.GetCommandState(CutCommandArgs args, System.Func<CommandState> nextHandler)
        {
            AssertIsForeground();
            return nextHandler();
        }

        void IChainedCommandHandler<CutCommandArgs>.ExecuteCommand(CutCommandArgs args, Action nextHandler, CommandExecutionContext context)
        {
            AssertIsForeground();
            DismissSessionIfActive();
            nextHandler();
        }

        CommandState IChainedCommandHandler<PasteCommandArgs>.GetCommandState(PasteCommandArgs args, Func<CommandState> nextHandler)
        {
            AssertIsForeground();
            return nextHandler();
        }

        void IChainedCommandHandler<PasteCommandArgs>.ExecuteCommand(PasteCommandArgs args, Action nextHandler, CommandExecutionContext context)
        {
            AssertIsForeground();
            DismissSessionIfActive();
            nextHandler();
        }
    }
}
