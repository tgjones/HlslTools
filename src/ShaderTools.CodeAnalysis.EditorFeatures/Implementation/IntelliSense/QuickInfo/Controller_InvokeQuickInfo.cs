// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.VisualStudio.Commanding;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text.Editor.Commanding.Commands;
using ShaderTools.CodeAnalysis.Editor.Shared.Extensions;

namespace ShaderTools.CodeAnalysis.Editor.Implementation.IntelliSense.QuickInfo
{
    internal partial class Controller
    {
        CommandState ICommandHandler<InvokeQuickInfoCommandArgs>.GetCommandState(InvokeQuickInfoCommandArgs args)
        {
            AssertIsForeground();
            return CommandState.Available;
        }

        bool ICommandHandler<InvokeQuickInfoCommandArgs>.ExecuteCommand(InvokeQuickInfoCommandArgs args, CommandExecutionContext context)
        {
            var caretPoint = args.TextView.GetCaretPoint(args.SubjectBuffer);
            if (caretPoint.HasValue)
            {
                // Invoking QuickInfo from the command, so there's no session yet.
                InvokeQuickInfo(caretPoint.Value.Position, trackMouse: false, augmentSession: null);
            }

            return true;
        }

        public void InvokeQuickInfo(int position, bool trackMouse, IQuickInfoSession augmentSession)
        {
            AssertIsForeground();
            DismissSessionIfActive();
            StartSession(position, trackMouse, augmentSession);
        }
    }
}
