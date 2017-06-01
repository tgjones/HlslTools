// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Language.Intellisense;
using ShaderTools.CodeAnalysis.Editor.Shared.Utilities;

namespace ShaderTools.CodeAnalysis.Editor.Implementation.IntelliSense.Completion.Presentation
{
    internal sealed class CompletionSource : ForegroundThreadAffinitizedObject, ICompletionSource
    {
        void ICompletionSource.AugmentCompletionSession(ICompletionSession session, IList<CompletionSet> completionSets)
        {
            AssertIsForeground();
            if (!session.Properties.TryGetProperty<CompletionPresenterSession>(CompletionPresenterSession.Key, out var presenterSession))
            {
                return;
            }

            session.Properties.RemoveProperty(CompletionPresenterSession.Key);
            presenterSession.AugmentCompletionSession(completionSets);
        }

        void IDisposable.Dispose()
        {
        }
    }
}
