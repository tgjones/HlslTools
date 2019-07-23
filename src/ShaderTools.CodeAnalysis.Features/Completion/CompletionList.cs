// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Text;
using ShaderTools.Utilities.Collections;

namespace ShaderTools.CodeAnalysis.Completion
{
    /// <summary>
    /// The set of completions to present to the user.
    /// </summary>
    internal sealed class CompletionList
    {
        /// <summary>
        /// The completion items to present to the user.
        /// </summary>
        public ImmutableArray<CompletionItem> Items { get; }

        /// <summary>
        /// The span of the syntax element at the caret position when the <see cref="CompletionList"/> 
        /// was created.
        /// 
        /// The span identifies the text in the document that is used to filter the initial list 
        /// presented to the user, and typically represents the region of the document that will 
        /// be changed if this item is committed.
        /// </summary>
        public TextSpan Span { get; }

        /// <summary>
        /// The rules used to control behavior of the completion list shown to the user during typing.
        /// </summary>
        public CompletionRules Rules { get; }

        /// <summary>
        /// An optional <see cref="CompletionItem"/> that appears selected in the list presented to the user during suggestion mode.
        /// Suggestion mode disables autoselection of items in the list, giving preference to the text typed by the user unless a specific item is selected manually.
        /// Specifying a <see cref="SuggestionModeItem"/> is a request that the completion host operate in suggestion mode.
        /// The item specified determines the text displayed and the description associated with it unless a different item is manually selected.
        /// No text is ever inserted when this item is completed, leaving the text the user typed instead.
        /// </summary>
        public CompletionItem SuggestionModeItem { get; }

        /// <summary>
        /// For testing purposes only.
        /// </summary>
        internal bool IsExclusive { get; }

        private CompletionList(
            TextSpan defaultSpan,
            ImmutableArray<CompletionItem> items,
            CompletionRules rules,
            CompletionItem suggestionModeItem,
            bool isExclusive)
        {
            Span = defaultSpan;

            Items = items.NullToEmpty();
            Rules = rules ?? CompletionRules.Default;
            SuggestionModeItem = suggestionModeItem;
            IsExclusive = isExclusive;

            foreach (var item in Items)
            {
                item.Span = defaultSpan;
            }
        }

        internal static CompletionList Create(
            TextSpan defaultSpan,
            ImmutableArray<CompletionItem> items,
            CompletionRules rules,
            CompletionItem suggestionModeItem,
            bool isExclusive)
        {
            return new CompletionList(defaultSpan, items, rules, suggestionModeItem, isExclusive);
        }

        /// <summary>
        /// The default <see cref="CompletionList"/> returned when no items are found to populate the list.
        /// </summary>
        public static readonly CompletionList Empty = new CompletionList(
            default, default, CompletionRules.Default,
            suggestionModeItem: null, isExclusive: false);
    }
}