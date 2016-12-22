using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using ShaderTools.Core.Text;
using ShaderTools.Editor.VisualStudio.Core.Glyphs;

namespace ShaderTools.Editor.VisualStudio.Hlsl.IntelliSense.Completion
{
    internal sealed class HlslCompletionSet : CompletionSet
    {
        private readonly ICompletionSession _session;
        private readonly CompletionModelManager _completionModelManager;
        private readonly DispatcherGlyphService _glyphService;

        public HlslCompletionSet(ICompletionSession session, CompletionModelManager completionModelManager, DispatcherGlyphService glyphService)
        {
            _session = session;
            _completionModelManager = completionModelManager;
            _glyphService = glyphService;
            _completionModelManager.ModelChanged += CompletionModelManagerOnModelChanged;
            _session.Dismissed += SessionOnDismissed;
            Refresh();
        }

        private void SessionOnDismissed(object sender, EventArgs e)
        {
            _session.Dismissed -= SessionOnDismissed;
            _completionModelManager.ModelChanged -= CompletionModelManagerOnModelChanged;
        }

        private void CompletionModelManagerOnModelChanged(object sender, EventArgs e)
        {
            Refresh();
        }

        private void Refresh()
        {
            Recalculate();
            Filter();
            SelectBestMatch();

            if (Completions.Count == 0)
                _session.Dismiss();
        }

        public override void SelectBestMatch()
        {
            var text = ApplicableTo.GetText(ApplicableTo.TextBuffer.CurrentSnapshot);
            if (string.IsNullOrWhiteSpace(text))
            {
                SelectionStatus = new CompletionSelectionStatus(null, false, false);
                return;
            }

            Microsoft.VisualStudio.Language.Intellisense.Completion bestMatch = null;
            var bestValue = 0;
            var isUnique = true;
            foreach (var completion in Completions)
            {
                int value = CompareCompletionText(completion.DisplayText, text);
                if (bestMatch == null || value > bestValue)
                {
                    bestMatch = completion;
                    bestValue = value;
                    isUnique = true;
                }
                else if (value == bestValue)
                {
                    isUnique = false;
                }
            }

            SelectionStatus = new CompletionSelectionStatus(bestMatch, bestValue > 0, isUnique);
        }

        private static int CompareCompletionText(string text, string pattern)
        {
            var position = text.IndexOf(pattern, StringComparison.InvariantCulture);
            if (position >= 0)
                return pattern.Length*2 + (position == 0 ? 1 : 0);
            position = text.IndexOf(pattern, StringComparison.CurrentCulture);
            if (position >= 0)
                return pattern.Length*2 + (position == 0 ? 1 : 0);
            position = text.IndexOf(pattern, StringComparison.InvariantCultureIgnoreCase);
            if (position >= 0)
                return pattern.Length + (position == 0 ? 1 : 0);
            position = text.IndexOf(pattern, StringComparison.CurrentCultureIgnoreCase);
            if (position >= 0)
                return pattern.Length + (position == 0 ? 1 : 0);
            return 0;
        }

        public override void Recalculate()
        {
            UpdateModel(_completionModelManager.Model);
        }

        private void UpdateModel(CompletionModel model)
        {
            ApplicableTo = ToTrackingSpan(model.ApplicableSpan);

            var builders = model.Items.Where(item => item.IsBuilder);
            UpdateBuilders(builders);

            var completions = model.Items.Where(item1 => !item1.IsBuilder);
            UpdateCompletions(completions);
        }

        private void UpdateBuilders(IEnumerable<CompletionItem> items)
        {
            WritableCompletionBuilders.BeginBulkOperation();
            try
            {
                WritableCompletionBuilders.Clear();
                WritableCompletionBuilders.AddRange(ToCompletions(items));
            }
            finally
            {
                WritableCompletionBuilders.EndBulkOperation();
            }
        }

        private void UpdateCompletions(IEnumerable<CompletionItem> items)
        {
            WritableCompletions.BeginBulkOperation();
            try
            {
                WritableCompletions.Clear();
                WritableCompletions.AddRange(ToCompletions(items));
            }
            finally
            {
                WritableCompletions.EndBulkOperation();
            }
        }

        private ITrackingSpan ToTrackingSpan(TextSpan span)
        {
            var snapshot = _session.TextView.TextBuffer.CurrentSnapshot;
            return snapshot.CreateTrackingSpan(span.Start, span.Length, SpanTrackingMode.EdgeInclusive);
        }

        private IEnumerable<Microsoft.VisualStudio.Language.Intellisense.Completion> ToCompletions(IEnumerable<CompletionItem> completionItems)
        {
            return completionItems.Select(ToCompletion);
        }

        private Microsoft.VisualStudio.Language.Intellisense.Completion ToCompletion(CompletionItem completionItem)
        {
            var displayText = completionItem.DisplayText;
            var insertionText = completionItem.InsertionText;
            var description = completionItem.Description;
            var image = ToImage(completionItem.Glyph);

            return new Microsoft.VisualStudio.Language.Intellisense.Completion(displayText, insertionText, description, image, null);
        }

        private ImageSource ToImage(Glyph? glyph)
        {
            return glyph?.GetImageSource(_glyphService);
        }
    }
}