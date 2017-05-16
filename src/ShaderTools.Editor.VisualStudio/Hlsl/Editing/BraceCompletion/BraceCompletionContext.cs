// Based on https://github.com/dotnet/roslyn/blob/master/src/EditorFeatures/CSharp/AutomaticCompletion/Sessions/CurlyBraceCompletionSession.cs
// Original licence follows:
// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.BraceCompletion;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.TextManager.Interop;
using ShaderTools.CodeAnalysis.Classification;
using ShaderTools.CodeAnalysis.Editor.Options;
using ShaderTools.CodeAnalysis.Formatting;
using ShaderTools.CodeAnalysis.Hlsl.Options;
using ShaderTools.CodeAnalysis.Options;
using ShaderTools.CodeAnalysis.Text;
using ShaderTools.Editor.VisualStudio.Core.Util.Extensions;
using ShaderTools.Editor.VisualStudio.Hlsl.Formatting;
using ShaderTools.Editor.VisualStudio.Hlsl.Options;
using ShaderTools.Editor.VisualStudio.Hlsl.Util.Extensions;
using ShaderTools.Utilities.Threading;
using TextSpan = ShaderTools.CodeAnalysis.Text.TextSpan;

namespace ShaderTools.Editor.VisualStudio.Hlsl.Editing.BraceCompletion
{
    internal sealed class BraceCompletionContext : IBraceCompletionContext
    {
        private readonly ISmartIndentationService _smartIndentationService;
        private readonly ITextBufferUndoManagerProvider _undoManager;
        private readonly IHlslOptionsService _optionsService;

        public BraceCompletionContext(
            ISmartIndentationService smartIndentationService, 
            ITextBufferUndoManagerProvider undoManager,
            IHlslOptionsService optionsService)
        {
            _smartIndentationService = smartIndentationService;
            _undoManager = undoManager;
            _optionsService = optionsService;
        }

        public void Start(IBraceCompletionSession session)
        {
            // If user has just typed opening brace of a type, then (depending on user settings) add a semicolon after the close brace.

            if (session.OpeningBrace != BraceKind.CurlyBrackets.Open)
                return;

            var document = session.SubjectBuffer.AsTextContainer().GetOpenDocumentInCurrentContext();
            var documentOptions = document.GetOptionsAsync().WaitAndGetResult(CancellationToken.None);

            if (documentOptions.GetOption(BraceCompletionOptions.AddSemicolonForTypes) && IsOpeningBraceOfType(session))
                session.SubjectBuffer.Insert(session.ClosingPoint.GetPosition(session.SubjectBuffer.CurrentSnapshot), ";");
        }

        private bool IsOpeningBraceOfType(IBraceCompletionSession session)
        {
            var snapshot = session.SubjectBuffer.CurrentSnapshot;
            var startPoint = session.OpeningPoint.GetPoint(snapshot);

            var document = snapshot.AsText().GetOpenDocumentInCurrentContextWithChanges();
            var syntaxTree = document.GetSyntaxTreeAsync(CancellationToken.None).WaitAndGetResult(CancellationToken.None); // TODO: Don't do this.

            var classificationService = document.LanguageServices.GetRequiredService<IClassificationService>();

            var classificationSpans = new List<ClassifiedSpan>();
            classificationService.AddSyntacticClassifications(syntaxTree, new TextSpan(startPoint - 1, 1), classificationSpans, CancellationToken.None);
            classificationSpans.RemoveAll(x => x.TextSpan.Start >= startPoint);
            classificationSpans.Reverse();

            foreach (var span in classificationSpans)
            {
                if (span.ClassificationType == ClassificationTypeNames.WhiteSpace)
                    continue;
                if (span.ClassificationType == ClassificationTypeNames.Identifier)
                    continue;

                if (span.ClassificationType == ClassificationTypeNames.Keyword)
                {
                    switch (new SnapshotSpan(snapshot, span.TextSpan.ToSpan()).GetText())
                    {
                        case "class":
                        case "struct":
                        case "interface":
                        case "cbuffer":
                            return true;

                        default:
                            return false;
                    }
                }

                return false;
            }

            return false;
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
            var document = session.SubjectBuffer.AsTextContainer().GetOpenDocumentInCurrentContext();
            var documentOptions = document.GetOptionsAsync().WaitAndGetResult(CancellationToken.None);

            if (!documentOptions.GetOption(FeatureOnOffOptions.AutoFormattingOnCloseBrace) && shouldHonorAutoFormattingOnCloseBraceOption)
                return;

            var snapshot = session.SubjectBuffer.CurrentSnapshot;
            var startPoint = session.OpeningPoint.GetPoint(snapshot);
            var endPoint = session.ClosingPoint.GetPoint(snapshot);
            var startPosition = startPoint.Position;
            var endPosition = endPoint.Position;

            if (documentOptions.GetOption(FormattingOptions.SmartIndent) == FormattingOptions.IndentStyle.Smart)
            {
                // skip whitespace
                while (startPosition >= 0 && char.IsWhiteSpace(snapshot[startPosition]))
                    startPosition--;

                // skip token
                startPosition--;
                while (startPosition >= 0 && !char.IsWhiteSpace(snapshot[startPosition]))
                    startPosition--;
            }

            session.SubjectBuffer.Format(
                TextSpan.FromBounds(Math.Max(startPosition, 0), endPosition),
                _optionsService);
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
            return _undoManager.GetTextBufferUndoManager(textView.TextBuffer).TextBufferUndoHistory;
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
            return true;
        }
    }
}