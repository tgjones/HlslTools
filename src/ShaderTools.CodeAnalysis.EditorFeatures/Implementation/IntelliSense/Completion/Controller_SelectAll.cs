// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.VisualStudio.Commanding;
using Microsoft.VisualStudio.Text.Editor.Commanding.Commands;

namespace ShaderTools.CodeAnalysis.Editor.Implementation.IntelliSense.Completion
{
    internal partial class Controller
    {
        CommandState IChainedCommandHandler<SelectAllCommandArgs>.GetCommandState(SelectAllCommandArgs args, Func<CommandState> nextHandler)
        {
            AssertIsForeground();
            return nextHandler();
        }

        void IChainedCommandHandler<SelectAllCommandArgs>.ExecuteCommand(SelectAllCommandArgs args, Action nextHandler, CommandExecutionContext context)
        {
            AssertIsForeground();
            DismissSessionIfActive();
            nextHandler();
        }
    }
}
