using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace HlslTools.VisualStudio.IntelliSense.QuickInfo
{
    internal sealed class QuickInfoSource : IQuickInfoSource
    {
        public void Dispose()
        {
        }

        public void AugmentQuickInfoSession(IQuickInfoSession session, IList<object> quickInfoContent, out ITrackingSpan applicableToSpan)
        {
            applicableToSpan = null;

            QuickInfoManager quickInfoManager;
            if (!session.Properties.TryGetProperty(typeof(QuickInfoManager), out quickInfoManager))
                return;

            var model = quickInfoManager.Model;
            var textSpan = model.Span;
            var span = new Span(textSpan.Start, textSpan.Length);
            var currentSnapshot = session.TextView.TextBuffer.CurrentSnapshot;
            var content = model.Text;
            if (content == null)
                return;

            if (!string.IsNullOrEmpty(model.Documentation))
                content += Environment.NewLine + model.Documentation;

            applicableToSpan = currentSnapshot.CreateTrackingSpan(span, SpanTrackingMode.EdgeNegative);
            quickInfoContent.Add(content);
        }
    }
}