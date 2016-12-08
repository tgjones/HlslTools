using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using ShaderTools.Core.Diagnostics;
using ShaderTools.Hlsl.Compilation;
using ShaderTools.VisualStudio.Core.ErrorList;
using ShaderTools.VisualStudio.Core.Parsing;
using ShaderTools.VisualStudio.Core.Util;
using ShaderTools.VisualStudio.Hlsl.Options;
using ShaderTools.VisualStudio.Hlsl.Parsing;
using ShaderTools.VisualStudio.Hlsl.Util.Extensions;

namespace ShaderTools.VisualStudio.Hlsl.ErrorList
{
    internal sealed class SemanticErrorManager : ErrorManager
    {
        public SemanticErrorManager(BackgroundParser backgroundParser, ITextView textView, IHlslOptionsService optionsService, IServiceProvider serviceProvider, ITextDocumentFactoryService textDocumentFactoryService) 
            : base(textView, optionsService, serviceProvider, textDocumentFactoryService)
        {
            backgroundParser.SubscribeToThrottledSemanticModelAvailable(BackgroundParserSubscriptionDelay.OnIdle,
                async x => await ExceptionHelper.TryCatchCancellation(() =>
                {
                    RefreshErrors(x.Snapshot, x.CancellationToken);
                    return Task.FromResult(0);
                }));
        }

        protected override IEnumerable<DiagnosticBase> GetDiagnostics(ITextSnapshot snapshot, CancellationToken cancellationToken)
        {
            SemanticModel semanticModel;
            if (!snapshot.TryGetSemanticModel(cancellationToken, out semanticModel))
                return Enumerable.Empty<DiagnosticBase>();
            return semanticModel.GetDiagnostics();
        }
    }
}