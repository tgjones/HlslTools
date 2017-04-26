using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using ShaderTools.CodeAnalysis.Diagnostics;
using ShaderTools.CodeAnalysis.Syntax;
using ShaderTools.Editor.VisualStudio.Core.ErrorList;
using ShaderTools.Editor.VisualStudio.Core.Parsing;
using ShaderTools.Editor.VisualStudio.Core.Util;
using ShaderTools.Editor.VisualStudio.Hlsl.Options;
using ShaderTools.Editor.VisualStudio.Hlsl.Parsing;
using ShaderTools.Editor.VisualStudio.Hlsl.Util.Extensions;

namespace ShaderTools.Editor.VisualStudio.Hlsl.ErrorList
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

        protected override Tuple<SyntaxTreeBase, IEnumerable<Diagnostic>> GetDiagnostics(ITextSnapshot snapshot, CancellationToken cancellationToken)
        {
            var syntaxTree = snapshot.GetSyntaxTree(cancellationToken);
            return Tuple.Create((SyntaxTreeBase) syntaxTree, syntaxTree.GetDiagnostics());
        }
    }
}