﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using HlslTools.Compilation;
using HlslTools.Diagnostics;
using HlslTools.VisualStudio.Options;
using HlslTools.VisualStudio.Parsing;
using HlslTools.VisualStudio.Util.Extensions;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace HlslTools.VisualStudio.ErrorList
{
    internal sealed class SemanticErrorManager : ErrorManager
    {
        public SemanticErrorManager(BackgroundParser backgroundParser, ITextView textView, IOptionsService optionsService, IServiceProvider serviceProvider, ITextDocumentFactoryService textDocumentFactoryService) 
            : base(textView, optionsService, serviceProvider, textDocumentFactoryService)
        {
            backgroundParser.SubscribeToThrottledSemanticModelAvailable(BackgroundParserSubscriptionDelay.OnIdle,
                x => RefreshErrors(x.Snapshot, x.CancellationToken));
        }

        protected override IEnumerable<Diagnostic> GetDiagnostics(ITextSnapshot snapshot, CancellationToken cancellationToken)
        {
            SemanticModel semanticModel;
            if (!snapshot.TryGetSemanticModel(cancellationToken, out semanticModel))
                return Enumerable.Empty<Diagnostic>();
            return semanticModel.GetDiagnostics();
        }
    }
}