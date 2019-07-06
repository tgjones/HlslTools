// Based on https://github.com/dotnet/roslyn/blob/master/src/EditorFeatures/CSharp/AutomaticCompletion/Sessions/CurlyBraceCompletionSession.cs
// Original licence follows:
// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Threading;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.BraceCompletion;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using ShaderTools.CodeAnalysis.Editor.Shared.Extensions;
using ShaderTools.CodeAnalysis.Formatting;
using ShaderTools.CodeAnalysis.Options;
using ShaderTools.CodeAnalysis.Text;
using ShaderTools.Utilities.Threading;
using TextSpan = Microsoft.CodeAnalysis.Text.TextSpan;

namespace ShaderTools.CodeAnalysis.Editor.Implementation.BraceCompletion
{
    internal sealed class BraceCompletionContext : IBraceCompletionContext
    {
        private readonly ISmartIndentationService _smartIndentationService;
        private readonly ITextBufferUndoManager _undoManager;
        private readonly IEditorBraceCompletionSession _editorSession;

        public BraceCompletionContext(
            ISmartIndentationService smartIndentationService,
            ITextBufferUndoManager undoManager,
            IEditorBraceCompletionSession editorSession)
        {
            _smartIndentationService = smartIndentationService;
            _undoManager = undoManager;
            _editorSession = editorSession;
        }

        public void Start(IBraceCompletionSession session)
        {
            _editorSession.AfterStart(session, CancellationToken.None);
        }

        public void Finish(IBraceCompletionSession session)
        {
            
        }

        public void OnReturn(IBraceCompletionSession session)
        {
            // check whether shape of the braces are what we support
            // shape must be either "{|}" or "{ }". | is where caret is. otherwise, we don't do any special behavior
            if (!ContainsOnlyWhitespace(session))
                return;

            // alright, it is in right shape.
            var undoHistory = GetUndoHistory(session.TextView);
            using (var transaction = undoHistory.CreateTransaction("Brace completion"))
            {
                session.SubjectBuffer.Insert(session.ClosingPoint.GetPosition(session.SubjectBuffer.CurrentSnapshot) - 1, Environment.NewLine);
                FormatTrackingSpan(session, false);

                // put caret at right indentation
                PutCaretOnLine(session, session.OpeningPoint.GetPoint(session.SubjectBuffer.CurrentSnapshot).GetContainingLine().LineNumber + 1);

                transaction.Complete();
            }
        }

        private void FormatTrackingSpan(IBraceCompletionSession session, bool shouldHonorAutoFormattingOnCloseBraceOption)
        {
            if (!session.SubjectBuffer.GetFeatureOnOffOption(FeatureOnOffOptions.AutoFormattingOnTyping))
                return;

            if (!session.SubjectBuffer.GetFeatureOnOffOption(FeatureOnOffOptions.AutoFormattingOnCloseBrace) && shouldHonorAutoFormattingOnCloseBraceOption)
                return;

            var snapshot = session.SubjectBuffer.CurrentSnapshot;
            var startPoint = session.OpeningPoint.GetPoint(snapshot);
            var endPoint = session.ClosingPoint.GetPoint(snapshot);
            var startPosition = startPoint.Position;
            var endPosition = endPoint.Position;

            var document = snapshot.GetOpenDocumentInCurrentContextWithChanges();
            var style = document != null 
                ? document.GetOptionsAsync(CancellationToken.None).WaitAndGetResult(CancellationToken.None).GetOption(FormattingOptions.SmartIndent)
                : FormattingOptions.SmartIndent.DefaultValue;

            if (style == FormattingOptions.IndentStyle.Smart)
            {
                // skip whitespace
                while (startPosition >= 0 && char.IsWhiteSpace(snapshot[startPosition]))
                    startPosition--;

                // skip token
                startPosition--;
                while (startPosition >= 0 && !char.IsWhiteSpace(snapshot[startPosition]))
                    startPosition--;
            }

            session.SubjectBuffer.Format(TextSpan.FromBounds(Math.Max(startPosition, 0), endPosition));
        }

        private void PutCaretOnLine(IBraceCompletionSession session, int lineNumber)
        {
            var lineOnSubjectBuffer = session.SubjectBuffer.CurrentSnapshot.GetLineFromLineNumber(lineNumber);

            var indentation = GetDesiredIndentation(session, lineOnSubjectBuffer);

            session.TextView.Caret.MoveTo(new VirtualSnapshotPoint(lineOnSubjectBuffer, indentation));
            session.TextView.Caret.EnsureVisible();
        }

        private int GetDesiredIndentation(IBraceCompletionSession session, ITextSnapshotLine lineOnSubjectBuffer)
        {
            // first try VS's smart indentation service
            var indentation = session.TextView.GetDesiredIndentation(_smartIndentationService, lineOnSubjectBuffer);
            if (indentation.HasValue)
                return indentation.Value;

            // do poor man's indentation
            var openingPoint = session.OpeningPoint.GetPoint(lineOnSubjectBuffer.Snapshot);
            var openingSpanLine = openingPoint.GetContainingLine();

            return openingPoint - openingSpanLine.Start;
        }

        private ITextUndoHistory GetUndoHistory(ITextView textView)
        {
            return _undoManager.TextBufferUndoHistory;
        }

        private static bool ContainsOnlyWhitespace(IBraceCompletionSession session)
        {
            var span = session.GetSessionSpan();

            var snapshot = span.Snapshot;

            var start = span.Start.Position;
            start = snapshot[start] == session.OpeningBrace ? start + 1 : start;

            var end = span.End.Position - 1;
            end = snapshot[end] == session.ClosingBrace ? end - 1 : end;

            if (!start.PositionInSnapshot(snapshot) || !end.PositionInSnapshot(snapshot))
                return false;

            for (int i = start; i <= end; i++)
            {
                if (!char.IsWhiteSpace(snapshot[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public bool AllowOverType(IBraceCompletionSession session)
        {
            return _editorSession.AllowOverType(session, CancellationToken.None);
        }
    }
}