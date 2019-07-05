// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.VisualStudio.Commanding;
using Microsoft.VisualStudio.Text.Editor.Commanding;
using Microsoft.VisualStudio.Text.Editor.Commanding.Commands;
using ShaderTools.CodeAnalysis.Editor.Implementation.IntelliSense.Completion;
using ShaderTools.CodeAnalysis.Editor.Shared.Utilities;

namespace ShaderTools.CodeAnalysis.Editor.CommandHandlers
{
    internal abstract class AbstractCompletionCommandHandler :
        ForegroundThreadAffinitizedObject,
        IChainedCommandHandler<TabKeyCommandArgs>,
        IChainedCommandHandler<ToggleCompletionModeCommandArgs>,
        IChainedCommandHandler<TypeCharCommandArgs>,
        IChainedCommandHandler<ReturnKeyCommandArgs>,
        IChainedCommandHandler<InvokeCompletionListCommandArgs>,
        IChainedCommandHandler<CommitUniqueCompletionListItemCommandArgs>,
        IChainedCommandHandler<PageUpKeyCommandArgs>,
        IChainedCommandHandler<PageDownKeyCommandArgs>,
        IChainedCommandHandler<CutCommandArgs>,
        IChainedCommandHandler<PasteCommandArgs>,
        IChainedCommandHandler<BackspaceKeyCommandArgs>,
        IChainedCommandHandler<SaveCommandArgs>,
        IChainedCommandHandler<DeleteKeyCommandArgs>,
        IChainedCommandHandler<SelectAllCommandArgs>
    {
        private readonly IAsyncCompletionService _completionService;

        public string DisplayName => "Completion";

        protected AbstractCompletionCommandHandler(IAsyncCompletionService completionService)
        {
            _completionService = completionService;
        }

        private bool TryGetController(EditorCommandArgs args, out Controller controller)
        {
            return _completionService.TryGetController(args.TextView, args.SubjectBuffer, out controller);
        }

        private bool TryGetControllerCommandHandler<TCommandArgs>(TCommandArgs args, out IChainedCommandHandler<TCommandArgs> commandHandler)
            where TCommandArgs : EditorCommandArgs
        {
            AssertIsForeground();
            if (!TryGetController(args, out var controller))
            {
                commandHandler = null;
                return false;
            }

            commandHandler = (IChainedCommandHandler<TCommandArgs>) controller;
            return true;
        }

        private CommandState GetCommandStateWorker<TCommandArgs>(
            TCommandArgs args,
            Func<CommandState> nextHandler)
            where TCommandArgs : EditorCommandArgs
        {
            AssertIsForeground();
            return TryGetControllerCommandHandler(args, out var commandHandler)
                ? commandHandler.GetCommandState(args, nextHandler)
                : nextHandler();
        }

        private void ExecuteCommandWorker<TCommandArgs>(
            TCommandArgs args,
            Action nextHandler,
            CommandExecutionContext context)
            where TCommandArgs : EditorCommandArgs
        {
            AssertIsForeground();
            if (TryGetControllerCommandHandler(args, out var commandHandler))
            {
                commandHandler.ExecuteCommand(args, nextHandler, context);
            }
            else
            {
                nextHandler();
            }
        }

        CommandState IChainedCommandHandler<TabKeyCommandArgs>.GetCommandState(TabKeyCommandArgs args, Func<CommandState> nextHandler)
        {
            AssertIsForeground();
            return GetCommandStateWorker(args, nextHandler);
        }

        void IChainedCommandHandler<TabKeyCommandArgs>.ExecuteCommand(TabKeyCommandArgs args, Action nextHandler, CommandExecutionContext context)
        {
            AssertIsForeground();
            ExecuteCommandWorker(args, nextHandler, context);
        }

        CommandState IChainedCommandHandler<ToggleCompletionModeCommandArgs>.GetCommandState(ToggleCompletionModeCommandArgs args, Func<CommandState> nextHandler)
        {
            AssertIsForeground();
            return GetCommandStateWorker(args, nextHandler);
        }

        void IChainedCommandHandler<ToggleCompletionModeCommandArgs>.ExecuteCommand(ToggleCompletionModeCommandArgs args, Action nextHandler, CommandExecutionContext context)
        {
            AssertIsForeground();
            ExecuteCommandWorker(args, nextHandler, context);
        }

        CommandState IChainedCommandHandler<TypeCharCommandArgs>.GetCommandState(TypeCharCommandArgs args, Func<CommandState> nextHandler)
        {
            AssertIsForeground();
            return GetCommandStateWorker(args, nextHandler);
        }

        void IChainedCommandHandler<TypeCharCommandArgs>.ExecuteCommand(TypeCharCommandArgs args, Action nextHandler, CommandExecutionContext context)
        {
            AssertIsForeground();
            ExecuteCommandWorker(args, nextHandler, context);
        }

        CommandState IChainedCommandHandler<ReturnKeyCommandArgs>.GetCommandState(ReturnKeyCommandArgs args, Func<CommandState> nextHandler)
        {
            AssertIsForeground();
            return GetCommandStateWorker(args, nextHandler);
        }

        void IChainedCommandHandler<ReturnKeyCommandArgs>.ExecuteCommand(ReturnKeyCommandArgs args, Action nextHandler, CommandExecutionContext context)
        {
            AssertIsForeground();
            ExecuteCommandWorker(args, nextHandler, context);
        }

        CommandState IChainedCommandHandler<InvokeCompletionListCommandArgs>.GetCommandState(InvokeCompletionListCommandArgs args, Func<CommandState> nextHandler)
        {
            AssertIsForeground();
            return GetCommandStateWorker(args, nextHandler);
        }

        void IChainedCommandHandler<InvokeCompletionListCommandArgs>.ExecuteCommand(InvokeCompletionListCommandArgs args, Action nextHandler, CommandExecutionContext context)
        {
            AssertIsForeground();
            ExecuteCommandWorker(args, nextHandler, context);
        }

        CommandState IChainedCommandHandler<PageUpKeyCommandArgs>.GetCommandState(PageUpKeyCommandArgs args, Func<CommandState> nextHandler)
        {
            AssertIsForeground();
            return GetCommandStateWorker(args, nextHandler);
        }

        void IChainedCommandHandler<PageUpKeyCommandArgs>.ExecuteCommand(PageUpKeyCommandArgs args, Action nextHandler, CommandExecutionContext context)
        {
            AssertIsForeground();
            ExecuteCommandWorker(args, nextHandler, context);
        }

        CommandState IChainedCommandHandler<PageDownKeyCommandArgs>.GetCommandState(PageDownKeyCommandArgs args, Func<CommandState> nextHandler)
        {
            AssertIsForeground();
            return GetCommandStateWorker(args, nextHandler);
        }

        void IChainedCommandHandler<PageDownKeyCommandArgs>.ExecuteCommand(PageDownKeyCommandArgs args, Action nextHandler, CommandExecutionContext context)
        {
            AssertIsForeground();
            ExecuteCommandWorker(args, nextHandler, context);
        }

        CommandState IChainedCommandHandler<CutCommandArgs>.GetCommandState(CutCommandArgs args, Func<CommandState> nextHandler)
        {
            AssertIsForeground();
            return GetCommandStateWorker(args, nextHandler);
        }

        void IChainedCommandHandler<CutCommandArgs>.ExecuteCommand(CutCommandArgs args, Action nextHandler, CommandExecutionContext context)
        {
            AssertIsForeground();
            ExecuteCommandWorker(args, nextHandler, context);
        }

        CommandState IChainedCommandHandler<PasteCommandArgs>.GetCommandState(PasteCommandArgs args, Func<CommandState> nextHandler)
        {
            AssertIsForeground();
            return GetCommandStateWorker(args, nextHandler);
        }

        void IChainedCommandHandler<PasteCommandArgs>.ExecuteCommand(PasteCommandArgs args, Action nextHandler, CommandExecutionContext context)
        {
            AssertIsForeground();
            ExecuteCommandWorker(args, nextHandler, context);
        }

        CommandState IChainedCommandHandler<CommitUniqueCompletionListItemCommandArgs>.GetCommandState(CommitUniqueCompletionListItemCommandArgs args, Func<CommandState> nextHandler)
        {
            AssertIsForeground();
            return GetCommandStateWorker(args, nextHandler);
        }

        void IChainedCommandHandler<CommitUniqueCompletionListItemCommandArgs>.ExecuteCommand(CommitUniqueCompletionListItemCommandArgs args, Action nextHandler, CommandExecutionContext context)
        {
            AssertIsForeground();
            ExecuteCommandWorker(args, nextHandler, context);
        }

        CommandState IChainedCommandHandler<BackspaceKeyCommandArgs>.GetCommandState(BackspaceKeyCommandArgs args, Func<CommandState> nextHandler)
        {
            AssertIsForeground();
            return GetCommandStateWorker(args, nextHandler);
        }

        void IChainedCommandHandler<BackspaceKeyCommandArgs>.ExecuteCommand(BackspaceKeyCommandArgs args, Action nextHandler, CommandExecutionContext context)
        {
            AssertIsForeground();
            ExecuteCommandWorker(args, nextHandler, context);
        }

        internal bool TryHandleEscapeKey(EscapeKeyCommandArgs commandArgs)
        {
            if (!TryGetController(commandArgs, out var controller))
            {
                return false;
            }

            return controller.TryHandleEscapeKey();
        }

        internal bool TryHandleUpKey(UpKeyCommandArgs commandArgs)
        {
            if (!TryGetController(commandArgs, out var controller))
            {
                return false;
            }

            return controller.TryHandleUpKey();
        }

        internal bool TryHandleDownKey(DownKeyCommandArgs commandArgs)
        {
            if (!TryGetController(commandArgs, out var controller))
            {
                return false;
            }

            return controller.TryHandleDownKey();
        }

        public CommandState GetCommandState(SaveCommandArgs args, Func<CommandState> nextHandler)
        {
            AssertIsForeground();
            return GetCommandStateWorker(args, nextHandler);
        }

        public void ExecuteCommand(SaveCommandArgs args, Action nextHandler, CommandExecutionContext context)
        {
            AssertIsForeground();
            ExecuteCommandWorker(args, nextHandler, context);
        }

        public CommandState GetCommandState(DeleteKeyCommandArgs args, Func<CommandState> nextHandler)
        {
            AssertIsForeground();
            return GetCommandStateWorker(args, nextHandler);
        }

        public void ExecuteCommand(DeleteKeyCommandArgs args, Action nextHandler, CommandExecutionContext context)
        {
            AssertIsForeground();
            ExecuteCommandWorker(args, nextHandler, context);
        }

        public CommandState GetCommandState(SelectAllCommandArgs args, Func<CommandState> nextHandler)
        {
            AssertIsForeground();
            return GetCommandStateWorker(args, nextHandler);
        }

        public void ExecuteCommand(SelectAllCommandArgs args, Action nextHandler, CommandExecutionContext context)
        {
            AssertIsForeground();
            ExecuteCommandWorker(args, nextHandler, context);
        }
    }
}
