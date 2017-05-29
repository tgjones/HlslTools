// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using ShaderTools.CodeAnalysis.Editor.Shared.Utilities;

namespace ShaderTools.CodeAnalysis.Editor.Implementation.IntelliSense.QuickInfo.Presentation
{
    [Export(typeof(IQuickInfoSourceProvider))]
    [Export(typeof(IIntelliSensePresenter<IQuickInfoPresenterSession, IQuickInfoSession>))]
    [Order]
    [Name(PredefinedQuickInfoPresenterNames.ShaderToolsQuickInfoPresenter)]
    [ContentType(ContentTypeNames.ShaderToolsContentType)]
    internal partial class QuickInfoPresenter : ForegroundThreadAffinitizedObject, IIntelliSensePresenter<IQuickInfoPresenterSession, IQuickInfoSession>, IQuickInfoSourceProvider
    {
        private static readonly object s_augmentSessionKey = new object();

        private readonly IQuickInfoBroker _quickInfoBroker;
        private readonly QuickInfoContentConverter _contentConverter;

        [ImportingConstructor]
        public QuickInfoPresenter(IQuickInfoBroker quickInfoBroker, QuickInfoContentConverter contentConverter)
        {
            _quickInfoBroker = quickInfoBroker;
            _contentConverter = contentConverter;
        }

        IQuickInfoPresenterSession IIntelliSensePresenter<IQuickInfoPresenterSession, IQuickInfoSession>.CreateSession(ITextView textView, ITextBuffer subjectBuffer, IQuickInfoSession sessionOpt)
        {
            AssertIsForeground();
            return new QuickInfoPresenterSession(_quickInfoBroker, textView, subjectBuffer, sessionOpt, _contentConverter);
        }

        IQuickInfoSource IQuickInfoSourceProvider.TryCreateQuickInfoSource(ITextBuffer textBuffer)
        {
            AssertIsForeground();
            return new QuickInfoSource();
        }
    }
}
