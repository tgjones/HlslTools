using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using ShaderTools.Core.Diagnostics;
using ShaderTools.VisualStudio.Core.ErrorList;
using ShaderTools.VisualStudio.Core.Parsing;
using ShaderTools.VisualStudio.Core.Util;
using ShaderTools.VisualStudio.Hlsl.Options;
using ShaderTools.VisualStudio.Hlsl.Parsing;
using ShaderTools.VisualStudio.Hlsl.Util.Extensions;

namespace ShaderTools.VisualStudio.Hlsl.ErrorList
{
    internal sealed class SyntaxErrorManager : ErrorManager
    {
        public SyntaxErrorManager(BackgroundParser backgroundParser, ITextView textView, IHlslOptionsService optionsService, IServiceProvider serviceProvider, ITextDocumentFactoryService textDocumentFactoryService)
            : base(textView, optionsService, serviceProvider, textDocumentFactoryService)
        {
            backgroundParser.SubscribeToThrottledSyntaxTreeAvailable(BackgroundParserSubscriptionDelay.OnIdle,
                async x => await ExceptionHelper.TryCatchCancellation(() =>
                {
                    RefreshErrors(x.Snapshot, x.CancellationToken);
                    return Task.FromResult(0);
                }));
        }

        protected override IEnumerable<DiagnosticBase> GetDiagnostics(ITextSnapshot snapshot, CancellationToken cancellationToken)
        {
            return snapshot.GetSyntaxTree(cancellationToken).GetDiagnostics();
        }
    }
}