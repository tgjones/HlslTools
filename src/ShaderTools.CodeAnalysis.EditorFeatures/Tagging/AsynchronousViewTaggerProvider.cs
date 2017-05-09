// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using ShaderTools.CodeAnalysis.Shared.TestHooks;

namespace ShaderTools.CodeAnalysis.Editor.Tagging
{
    internal abstract class AsynchronousViewTaggerProvider<TTag> : AbstractAsynchronousTaggerProvider<TTag>,
        IViewTaggerProvider
        where TTag : ITag
    {
        protected AsynchronousViewTaggerProvider(
            IAsynchronousOperationListener asyncListener,
            IForegroundNotificationService notificationService)
                : base(asyncListener, notificationService)
        {
        }

        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer subjectBuffer) where T : ITag
        {
            if (textView == null)
            {
                throw new ArgumentNullException(nameof(subjectBuffer));
            }

            if (subjectBuffer == null)
            {
                throw new ArgumentNullException(nameof(subjectBuffer));
            }

            return this.CreateTaggerWorker<T>(textView, subjectBuffer);
        }

        ITagger<T> IViewTaggerProvider.CreateTagger<T>(ITextView textView, ITextBuffer buffer)
        {
            return CreateTagger<T>(textView, buffer);
        }
    }
}
