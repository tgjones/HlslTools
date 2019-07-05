// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Commanding;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor.Commanding.Commands;
using Microsoft.VisualStudio.Utilities;
using ShaderTools.CodeAnalysis.Editor.Properties;
using ShaderTools.CodeAnalysis.Editor.Shared.Extensions;
using ShaderTools.CodeAnalysis.Notification;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.Editor.GoToDefinition
{
    [Export(typeof(ICommandHandler))]
    [ContentType(ContentTypeNames.ShaderToolsContentType)]
    [Name(nameof(GoToDefinitionCommandHandler))]
    internal class GoToDefinitionCommandHandler :
        ICommandHandler<GoToDefinitionCommandArgs>
    {
        public string DisplayName => "Go To Definition";

        private (Document, IEditorGoToDefinitionService) GetDocumentAndService(ITextSnapshot snapshot)
        {
            var document = snapshot.GetOpenDocumentInCurrentContextWithChanges();
            return (document, document?.Workspace.Services.GetService<IEditorGoToDefinitionService>());
        }

        public CommandState GetCommandState(GoToDefinitionCommandArgs args)
        {
            var (_, service) = GetDocumentAndService(args.SubjectBuffer.CurrentSnapshot);
            return service != null
                ? CommandState.Available
                : CommandState.Unavailable;
        }

        public bool ExecuteCommand(GoToDefinitionCommandArgs args, CommandExecutionContext context)
        {
            var subjectBuffer = args.SubjectBuffer;
            var (document, service) = GetDocumentAndService(subjectBuffer.CurrentSnapshot);
            if (service != null)
            {
                var caretPos = args.TextView.GetCaretPoint(subjectBuffer);
                if (caretPos.HasValue && TryExecuteCommand(document, caretPos.Value, service, context))
                {
                    return true;
                }
            }

            return false;
        }

        private bool TryExecuteCommand(Document document, int caretPosition, IEditorGoToDefinitionService goToDefinitionService, CommandExecutionContext context)
        {
            string errorMessage = null;

            using (context.OperationContext.AddScope(true, EditorFeaturesResources.Navigating_to_definition))
            {
                if (goToDefinitionService != null &&
                    goToDefinitionService.TryGoToDefinition(document, caretPosition, context.OperationContext.UserCancellationToken))
                {
                    return true;
                }

                errorMessage = EditorFeaturesResources.Cannot_navigate_to_the_symbol_under_the_caret;
            }

            if (errorMessage != null)
            {
                context.OperationContext.TakeOwnership();
                var workspace = document.Workspace;
                var notificationService = workspace.Services.GetService<INotificationService>();
                notificationService.SendNotification(errorMessage, title: EditorFeaturesResources.Go_to_Definition, severity: NotificationSeverity.Information);
            }

            return true;
        }
    }
}