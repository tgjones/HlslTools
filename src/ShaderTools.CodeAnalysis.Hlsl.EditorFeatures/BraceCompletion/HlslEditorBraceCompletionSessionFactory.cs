using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis.Classification;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.BraceCompletion;
using ShaderTools.CodeAnalysis.Classification;
using ShaderTools.CodeAnalysis.Editor.Implementation.BraceCompletion;
using ShaderTools.CodeAnalysis.Editor.Options;
using ShaderTools.CodeAnalysis.Text.Shared.Extensions;
using ShaderTools.Utilities.Threading;

namespace ShaderTools.CodeAnalysis.Editor.Hlsl.BraceCompletion
{
    [ExportLanguageService(typeof(IEditorBraceCompletionSessionFactory), LanguageNames.Hlsl)]
    internal sealed class HlslEditorBraceCompletionSessionFactory : IEditorBraceCompletionSessionFactory
    {
        public IEditorBraceCompletionSession TryCreateSession(Document document, int openingPosition, char openingBrace, CancellationToken cancellationToken)
        {
            return new HlslEditorBraceCompletionSession(document);
        }

        private sealed class HlslEditorBraceCompletionSession : IEditorBraceCompletionSession
        {
            private readonly Document _document;

            public HlslEditorBraceCompletionSession(Document document)
            {
                _document = document;
            }

            public void AfterReturn(IBraceCompletionSession session, CancellationToken cancellationToken)
            {
                
            }

            public void AfterStart(IBraceCompletionSession session, CancellationToken cancellationToken)
            {
                // If user has just typed opening brace of a type, then (depending on user settings) add a semicolon after the close brace.

                if (session.OpeningBrace != BraceKind.CurlyBrackets.Open)
                    return;

                var documentOptions = _document.GetOptionsAsync().WaitAndGetResult(CancellationToken.None);

                if (documentOptions.GetOption(BraceCompletionOptions.AddSemicolonForTypes) && IsOpeningBraceOfType(session))
                    session.SubjectBuffer.Insert(session.ClosingPoint.GetPosition(session.SubjectBuffer.CurrentSnapshot), ";");
            }

            private bool IsOpeningBraceOfType(IBraceCompletionSession session)
            {
                var snapshot = session.SubjectBuffer.CurrentSnapshot;
                var startPoint = session.OpeningPoint.GetPoint(snapshot);

                var syntaxTree = _document.GetSyntaxTreeAsync(CancellationToken.None).WaitAndGetResult(CancellationToken.None); // TODO: Don't do this.

                var classificationService = _document.LanguageServices.GetRequiredService<IClassificationService>();

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

            public bool AllowOverType(IBraceCompletionSession session, CancellationToken cancellationToken)
            {
                return true;
            }

            public bool CheckOpeningPoint(IBraceCompletionSession session, CancellationToken cancellationToken)
            {
                return true;
            }
        }
    }
}
