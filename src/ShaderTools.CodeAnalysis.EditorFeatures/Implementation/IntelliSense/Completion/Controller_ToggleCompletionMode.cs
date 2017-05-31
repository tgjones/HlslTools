// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using ShaderTools.CodeAnalysis.Editor.Commands;
using ShaderTools.CodeAnalysis.Editor.Options;
using ShaderTools.CodeAnalysis.Editor.Shared.Extensions;
using ShaderTools.CodeAnalysis.Options;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.Editor.Implementation.IntelliSense.Completion
{
    internal partial class Controller
    {
        CommandState ICommandHandler<ToggleCompletionModeCommandArgs>.GetCommandState(ToggleCompletionModeCommandArgs args, System.Func<CommandState> nextHandler)
        {
            AssertIsForeground();

            var isEnabled = args.SubjectBuffer.GetFeatureOnOffOption(EditorCompletionOptions.UseSuggestionMode);
            return new CommandState(isAvailable: true, isChecked: isEnabled);
        }

        void ICommandHandler<ToggleCompletionModeCommandArgs>.ExecuteCommand(ToggleCompletionModeCommandArgs args, Action nextHandler)
        {
            if (Workspace.TryGetWorkspace(args.SubjectBuffer.AsTextContainer(), out var workspace))
            {
                Option<bool> option = EditorCompletionOptions.UseSuggestionMode;

                var newState = !workspace.Options.GetOption(option);
                workspace.Options = workspace.Options.WithChangedOption(option, newState);

                // If we don't have a computation in progress, then we don't have to do anything here.
                if (this.sessionOpt == null)
                {
                    return;
                }

                this.sessionOpt.SetModelBuilderState(newState);
            }
        }
    }
}
