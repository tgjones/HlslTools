// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.Options;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using ShaderTools.CodeAnalysis.Diagnostics;
using ShaderTools.CodeAnalysis.Editor.Tagging;
using ShaderTools.CodeAnalysis.Notification;
using ShaderTools.CodeAnalysis.Options;
using ShaderTools.Utilities.Diagnostics;

namespace ShaderTools.CodeAnalysis.Editor.Shared.Tagging
{
    internal static partial class 
        TaggerEventSources
    {
        public static ITaggerEventSource Compose(
            params ITaggerEventSource[] eventSources)
        {
            return new CompositionEventSource(eventSources);
        }

        public static ITaggerEventSource Compose(IEnumerable<ITaggerEventSource> eventSources)
        {
            return new CompositionEventSource(eventSources.ToArray());
        }

        public static ITaggerEventSource OnCaretPositionChanged(ITextView textView, ITextBuffer subjectBuffer, TaggerDelay delay)
        {
            return new CaretPositionChangedEventSource(textView, subjectBuffer, delay);
        }

        public static ITaggerEventSource OnTextChanged(ITextBuffer subjectBuffer, TaggerDelay delay)
        {
            Contract.ThrowIfNull(subjectBuffer);

            return new TextChangedEventSource(subjectBuffer, delay);
        }

        /// <summary>
        /// Reports an event any time the semantics have changed such that this 
        /// <paramref name="subjectBuffer"/> should be retagged.  Semantics are considered changed 
        /// for a buffer if an edit happens directly in that buffer, or if a top level visible 
        /// change happens in any sibling document or in any dependent projects' documents.
        /// </summary>
        public static ITaggerEventSource OnSemanticChanged(ITextBuffer subjectBuffer, TaggerDelay delay, ISemanticChangeNotificationService notificationService)
        {
            return new SemanticChangedEventSource(subjectBuffer, delay, notificationService);
        }

        public static ITaggerEventSource OnOptionChanged(
            ITextBuffer subjectBuffer,
            IOption option,
            TaggerDelay delay)
        {
            return new OptionChangedEventSource(subjectBuffer, option, delay);
        }

        public static ITaggerEventSource OnDiagnosticsChanged(
            ITextBuffer subjectBuffer,
            IDiagnosticService service,
            TaggerDelay delay)
        {
            return new DiagnosticsChangedEventSource(subjectBuffer, service, delay);
        }

        public static ITaggerEventSource OnViewSpanChanged(ITextView textView, TaggerDelay textChangeDelay, TaggerDelay scrollChangeDelay)
        {
            return new ViewSpanChangedEventSource(textView, textChangeDelay, scrollChangeDelay);
        }
    }
}