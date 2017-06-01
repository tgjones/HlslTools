// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using ShaderTools.CodeAnalysis.Editor.Shared.Utilities;

namespace ShaderTools.CodeAnalysis.Editor.Implementation.IntelliSense.Completion.Presentation
{
    [Export(typeof(IIntelliSensePresenter<ICompletionPresenterSession, ICompletionSession>))]
    [Export(typeof(ICompletionSourceProvider))]
    [Name(PredefinedCompletionPresenterNames.RoslynCompletionPresenter)]
    [ContentType(ContentTypeNames.ShaderToolsContentType)]
    internal sealed class CompletionPresenter : ForegroundThreadAffinitizedObject, IIntelliSensePresenter<ICompletionPresenterSession, ICompletionSession>, ICompletionSourceProvider
    {
        private readonly ICompletionBroker _completionBroker;
        private readonly IGlyphService _glyphService;

        [ImportingConstructor]
        public CompletionPresenter(
            ICompletionBroker completionBroker,
            IGlyphService glyphService)
        {
            _completionBroker = completionBroker;
            _glyphService = glyphService;
        }

        ICompletionPresenterSession IIntelliSensePresenter<ICompletionPresenterSession, ICompletionSession>.CreateSession(
            ITextView textView, ITextBuffer subjectBuffer, ICompletionSession session)
        {
            AssertIsForeground();

            return new CompletionPresenterSession(
                _completionBroker, _glyphService, textView, subjectBuffer);
        }

        ICompletionSource ICompletionSourceProvider.TryCreateCompletionSource(ITextBuffer textBuffer)
        {
            AssertIsForeground();
            return new CompletionSource();
        }
    }
}
