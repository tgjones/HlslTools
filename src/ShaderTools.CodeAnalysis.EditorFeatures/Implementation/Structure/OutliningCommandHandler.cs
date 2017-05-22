// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Outlining;
using ShaderTools.CodeAnalysis.Editor.Commands;

namespace ShaderTools.CodeAnalysis.Editor.Implementation.Structure
{
    [ExportCommandHandler("Outlining Command Handler", ContentTypeNames.ShaderToolsContentType)]
    internal sealed class OutliningCommandHandler : ICommandHandler<StartAutomaticOutliningCommandArgs>
    {
        private readonly IOutliningManagerService _outliningManagerService;

        [ImportingConstructor]
        public OutliningCommandHandler(IOutliningManagerService outliningManagerService)
        {
            _outliningManagerService = outliningManagerService;
        }

        public void ExecuteCommand(StartAutomaticOutliningCommandArgs args, Action nextHandler)
        {
            // The editor actually handles this command, we just have to make sure it is enabled.
            nextHandler();
        }

        public CommandState GetCommandState(StartAutomaticOutliningCommandArgs args, Func<CommandState> nextHandler)
        {
            var outliningManager = _outliningManagerService.GetOutliningManager(args.TextView);
            var enabled = false;
            if (outliningManager != null)
            {
                enabled = outliningManager.Enabled;
            }

            return new CommandState(isAvailable: !enabled);
        }
    }
}
