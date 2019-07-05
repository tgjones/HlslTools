// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.VisualStudio.Commanding;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor.Commanding;
using Microsoft.VisualStudio.Text.Editor.Commanding.Commands;
using Microsoft.VisualStudio.Utilities;
using ShaderTools.CodeAnalysis.Editor.Implementation.IntelliSense.QuickInfo;
using ShaderTools.CodeAnalysis.Editor.Shared.Utilities;
using ShaderTools.CodeAnalysis.QuickInfo;
using ShaderTools.CodeAnalysis.Shared.TestHooks;
using ShaderTools.CodeAnalysis.Shared.Utilities;

namespace ShaderTools.CodeAnalysis.Editor.CommandHandlers
{
    [Export]
    [Export(typeof(IQuickInfoSourceProvider))]
    [Order(After = PredefinedQuickInfoPresenterNames.ShaderToolsQuickInfoPresenter)]
    [Export(typeof(ICommandHandler))]
    [ContentType(ContentTypeNames.ShaderToolsContentType)]
    [Name(nameof(QuickInfoCommandHandlerAndSourceProvider))]
    internal partial class QuickInfoCommandHandlerAndSourceProvider :
        ForegroundThreadAffinitizedObject,
        ICommandHandler<InvokeQuickInfoCommandArgs>,
        IQuickInfoSourceProvider
    {
        //private readonly IInlineRenameService _inlineRenameService;
        private readonly IIntelliSensePresenter<IQuickInfoPresenterSession, IQuickInfoSession> _presenter;
        private readonly IEnumerable<Lazy<IAsynchronousOperationListener, FeatureMetadata>> _asyncListeners;
        private readonly IQuickInfoProviderCoordinatorFactory _providerCoordinatorFactory;

        public string DisplayName => "Quick Info";

        [ImportingConstructor]
        public QuickInfoCommandHandlerAndSourceProvider(
            //IInlineRenameService inlineRenameService,
            IQuickInfoProviderCoordinatorFactory providerCoordinatorFactory,
            [ImportMany] IEnumerable<Lazy<IAsynchronousOperationListener, FeatureMetadata>> asyncListeners,
            [ImportMany] IEnumerable<Lazy<IIntelliSensePresenter<IQuickInfoPresenterSession, IQuickInfoSession>, OrderableMetadata>> presenters)
            : this(//inlineRenameService,
                   ExtensionOrderer.Order(presenters).Select(lazy => lazy.Value).FirstOrDefault(),
                   providerCoordinatorFactory, asyncListeners)
        {
        }

        // For testing purposes.
        public QuickInfoCommandHandlerAndSourceProvider(
            //IInlineRenameService inlineRenameService,
            IIntelliSensePresenter<IQuickInfoPresenterSession, IQuickInfoSession> presenter,
            IQuickInfoProviderCoordinatorFactory providerCoordinatorFactory,
            [ImportMany] IEnumerable<Lazy<IAsynchronousOperationListener, FeatureMetadata>> asyncListeners)
        {
            //_inlineRenameService = inlineRenameService;
            _providerCoordinatorFactory = providerCoordinatorFactory;
            _asyncListeners = asyncListeners;
            _presenter = presenter;
        }

        private bool TryGetController(EditorCommandArgs args, out Controller controller)
        {
            AssertIsForeground();

            //// check whether this feature is on.
            //if (!args.SubjectBuffer.GetFeatureOnOffOption(InternalFeatureOnOffOptions.QuickInfo))
            //{
            //    controller = null;
            //    return false;
            //}

            // If we don't have a presenter, then there's no point in us even being involved.  Just
            // defer to the next handler in the chain.
            if (_presenter == null)
            {
                controller = null;
                return false;
            }

            // TODO(cyrusn): If there are no presenters then we should not create a controller.
            // Otherwise we'll be affecting the user's typing and they'll have no idea why :)
            controller = Controller.GetInstance(
                args, _presenter,
                new AggregateAsynchronousOperationListener(_asyncListeners, FeatureAttribute.QuickInfo),
                _providerCoordinatorFactory);
            return true;
        }

        private bool TryGetControllerCommandHandler<TCommandArgs>(TCommandArgs args, out ICommandHandler<TCommandArgs> commandHandler)
            where TCommandArgs : EditorCommandArgs
        {
            AssertIsForeground();
            if (!TryGetController(args, out var controller))
            {
                commandHandler = null;
                return false;
            }

            commandHandler = (ICommandHandler<TCommandArgs>)controller;
            return true;
        }

        private CommandState GetCommandStateWorker<TCommandArgs>(
            TCommandArgs args)
            where TCommandArgs : EditorCommandArgs
        {
            AssertIsForeground();
            return TryGetControllerCommandHandler(args, out var commandHandler)
                ? commandHandler.GetCommandState(args)
                : CommandState.Unspecified;
        }

        private bool ExecuteCommandWorker<TCommandArgs>(
            TCommandArgs args,
            CommandExecutionContext context)
            where TCommandArgs : EditorCommandArgs
        {
            AssertIsForeground();
            if (TryGetControllerCommandHandler(args, out var commandHandler))
            {
                commandHandler.ExecuteCommand(args, context);
                return true;
            }
            return false;
        }

        CommandState ICommandHandler<InvokeQuickInfoCommandArgs>.GetCommandState(InvokeQuickInfoCommandArgs args)
        {
            AssertIsForeground();
            return GetCommandStateWorker(args);
        }

        bool ICommandHandler<InvokeQuickInfoCommandArgs>.ExecuteCommand(InvokeQuickInfoCommandArgs args, CommandExecutionContext context)
        {
            AssertIsForeground();
            return ExecuteCommandWorker(args, context);
        }

        public IQuickInfoSource TryCreateQuickInfoSource(ITextBuffer textBuffer)
        {
            return new QuickInfoSource(this, textBuffer);
        }

        internal bool TryHandleEscapeKey(EscapeKeyCommandArgs commandArgs)
        {
            if (!TryGetController(commandArgs, out var controller))
            {
                return false;
            }

            return controller.TryHandleEscapeKey();
        }
    }
}
