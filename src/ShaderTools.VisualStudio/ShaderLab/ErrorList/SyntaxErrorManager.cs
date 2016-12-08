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
using ShaderTools.VisualStudio.ShaderLab.Options;
using ShaderTools.VisualStudio.ShaderLab.Parsing;
using ShaderTools.VisualStudio.ShaderLab.Util.Extensions;

namespace ShaderTools.VisualStudio.ShaderLab.ErrorList
{
    internal sealed class SyntaxErrorManager : ErrorManager
    {
        public SyntaxErrorManager(BackgroundParser backgroundParser, ITextView textView, IShaderLabOptionsService optionsService, IServiceProvider serviceProvider, ITextDocumentFactoryService textDocumentFactoryService)
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