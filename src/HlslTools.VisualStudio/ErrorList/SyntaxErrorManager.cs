using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HlslTools.Diagnostics;
using HlslTools.VisualStudio.Options;
using HlslTools.VisualStudio.Parsing;
using HlslTools.VisualStudio.Util;
using HlslTools.VisualStudio.Util.Extensions;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace HlslTools.VisualStudio.ErrorList
{
    internal sealed class SyntaxErrorManager : ErrorManager
    {
        public SyntaxErrorManager(BackgroundParser backgroundParser, ITextView textView, IOptionsService optionsService, IServiceProvider serviceProvider, ITextDocumentFactoryService textDocumentFactoryService)
            : base(textView, optionsService, serviceProvider, textDocumentFactoryService)
        {
            backgroundParser.SubscribeToThrottledSyntaxTreeAvailable(BackgroundParserSubscriptionDelay.OnIdle,
                async x => await ExceptionHelper.TryCatchCancellation(() =>
                {
                    RefreshErrors(x.Snapshot, x.CancellationToken);
                    return Task.FromResult(0);
                }));
        }

        protected override IEnumerable<Diagnostic> GetDiagnostics(ITextSnapshot snapshot, CancellationToken cancellationToken)
        {
            return snapshot.GetSyntaxTree(cancellationToken).GetDiagnostics();
        }
    }
}