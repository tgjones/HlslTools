using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using ShaderTools.CodeAnalysis.Completion;
using ShaderTools.CodeAnalysis.Text;
using VSCompletion = Microsoft.VisualStudio.Language.Intellisense.Completion;

namespace ShaderTools.CodeAnalysis.Editor.Implementation.IntelliSense.Completion.Presentation
{
    internal sealed class VisualStudioCompletionSet : CompletionSet
    {
        private readonly ITextView _textView;

        protected readonly ITextBuffer SubjectBuffer;
        protected readonly CompletionPresenterSession CompletionPresenterSession;

        protected Dictionary<CompletionItem, VSCompletion> CompletionItemMap;
        protected CompletionItem SuggestionModeItem;

        protected string FilterText;

        public VisualStudioCompletionSet(
            CompletionPresenterSession completionPresenterSession,
            ITextView textView,
            ITextBuffer subjectBuffer)
        {
            CompletionPresenterSession = completionPresenterSession;
            _textView = textView;
            SubjectBuffer = subjectBuffer;
        }

        public override void SelectBestMatch()
        {
            // Do nothing.  We do *not* want the default behavior that the editor has.  We've
            // already computed the best match.
        }

        public override void Filter()
        {
            // Do nothing.  We do *not* want the default behavior that the editor has.  We've
            // already filtered the list.
        }

        public override void Recalculate()
        {
            // Do nothing.  Our controller will already recalculate if necessary.
        }

        public void SetTrackingSpan(ITrackingSpan trackingSpan)
        {
            ApplicableTo = trackingSpan;
        }

        public void SetCompletionItems(
            IList<CompletionItem> completionItems,
            CompletionItem selectedItem,
            CompletionItem suggestionModeItem,
            bool suggestionMode,
            bool isSoftSelected,
            ImmutableArray<CompletionItemFilter> completionItemFilters,
            string filterText)
        {
            // Initialize the completion map to a reasonable default initial size (+1 for the builder)
            CompletionItemMap = CompletionItemMap ?? new Dictionary<CompletionItem, VSCompletion>(completionItems.Count + 1);
            FilterText = filterText;
            SuggestionModeItem = suggestionModeItem;

            CreateCompletionListBuilder(selectedItem, suggestionModeItem, suggestionMode);
            CreateNormalCompletionListItems(completionItems);

            var selectedCompletionItem = selectedItem != null ? GetVSCompletion(selectedItem) : null;
            SelectionStatus = new CompletionSelectionStatus(
                selectedCompletionItem,
                isSelected: !isSoftSelected, isUnique: selectedCompletionItem != null);
        }

        private void CreateCompletionListBuilder(
            CompletionItem selectedItem,
            CompletionItem suggestionModeItem,
            bool suggestionMode)
        {
            try
            {
                WritableCompletionBuilders.BeginBulkOperation();
                WritableCompletionBuilders.Clear();

                if (suggestionMode)
                {
                    var applicableToText = ApplicableTo.GetText(
                        ApplicableTo.TextBuffer.CurrentSnapshot);

                    var text = applicableToText.Length > 0 ? applicableToText : suggestionModeItem.DisplayText;
                    var vsCompletion = GetVSCompletion(suggestionModeItem, text);

                    WritableCompletionBuilders.Add(vsCompletion);
                }
            }
            finally
            {
                WritableCompletionBuilders.EndBulkOperation();
            }
        }

        private void CreateNormalCompletionListItems(IList<CompletionItem> completionItems)
        {
            try
            {
                WritableCompletions.BeginBulkOperation();
                WritableCompletions.Clear();

                foreach (var item in completionItems)
                {
                    var completionItem = GetVSCompletion(item);
                    WritableCompletions.Add(completionItem);
                }
            }
            finally
            {
                WritableCompletions.EndBulkOperation();
            }
        }

        private VSCompletion GetVSCompletion(CompletionItem item, string displayText = null)
        {
            if (!CompletionItemMap.TryGetValue(item, out var value))
            {
                value = new CustomCommitCompletion(CompletionPresenterSession, item);
                CompletionItemMap.Add(item, value);
            }

            value.DisplayText = displayText ?? item.DisplayText;

            return value;
        }

        public CompletionItem GetCompletionItem(VSCompletion completion)
        {
            // Linear search is ok since this is only called by the user manually selecting 
            // an item.  Creating a reverse mapping uses too much memory and affects GCs.
            foreach (var kvp in CompletionItemMap)
            {
                if (kvp.Value == completion)
                {
                    return kvp.Key;
                }
            }

            return null;
        }
    }
}
