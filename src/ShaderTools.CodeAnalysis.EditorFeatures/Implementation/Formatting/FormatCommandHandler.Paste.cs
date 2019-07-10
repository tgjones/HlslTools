// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Threading;
using Microsoft.VisualStudio.Commanding;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor.Commanding.Commands;
using ShaderTools.CodeAnalysis.Editor.Properties;
using ShaderTools.CodeAnalysis.Editor.Shared.Extensions;
using ShaderTools.CodeAnalysis.Options;
using ShaderTools.CodeAnalysis.Shared.Extensions;
using ShaderTools.CodeAnalysis.Text;
using ShaderTools.Utilities.Threading;

namespace ShaderTools.CodeAnalysis.Editor.Implementation.Formatting
{
    internal partial class FormatCommandHandler
    {
        public CommandState GetCommandState(PasteCommandArgs args, Func<CommandState> nextHandler)
        {
            return nextHandler();
        }

        public void ExecuteCommand(PasteCommandArgs args, Action nextHandler, CommandExecutionContext context)
        {
            using (context.OperationContext.AddScope(allowCancellation: true, EditorFeaturesResources.Formatting_pasted_text))
            {
                ExecuteCommandWorker(args, nextHandler, context.OperationContext.UserCancellationToken);
            }
        }

        private void ExecuteCommandWorker(PasteCommandArgs args, Action nextHandler, CancellationToken cancellationToken)
        {
            var caretPosition = args.TextView.GetCaretPoint(args.SubjectBuffer);

            nextHandler();

            //if (!args.SubjectBuffer.CanApplyChangeDocumentToWorkspace())
            //{
            //    return;
            //}

            if (!args.SubjectBuffer.GetFeatureOnOffOption(FeatureOnOffOptions.FormatOnPaste) ||
                !caretPosition.HasValue)
            {
                return;
            }

            var trackingSpan = caretPosition.Value.Snapshot.CreateTrackingSpan(caretPosition.Value.Position, 0, SpanTrackingMode.EdgeInclusive);

            var document = args.SubjectBuffer.CurrentSnapshot.GetOpenDocumentInCurrentContextWithChanges();
            if (document == null)
            {
                return;
            }

            var formattingService = document.GetLanguageService<IEditorFormattingService>();
            if (formattingService == null || !formattingService.SupportsFormatOnPaste)
            {
                return;
            }

            var span = trackingSpan.GetSpan(args.SubjectBuffer.CurrentSnapshot).Span.ToTextSpan();
            var changes = formattingService.GetFormattingChangesOnPasteAsync(document, span, cancellationToken).WaitAndGetResult(cancellationToken);
            if (changes.Count == 0)
            {
                return;
            }

            document.Workspace.ApplyTextChanges(document.Id, changes, cancellationToken);
        }
    }
}
