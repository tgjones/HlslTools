// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Text;
using ShaderTools.CodeAnalysis.Options;

namespace ShaderTools.CodeAnalysis.Completion
{
    /// <summary>
    /// The context presented to a <see cref="CompletionProvider"/> when providing completions.
    /// </summary>
    internal sealed class CompletionContext
    {
        private readonly List<CompletionItem> _items;

        internal IReadOnlyList<CompletionItem> Items => _items;

        internal CompletionProvider Provider { get; }

        /// <summary>
        /// The document that completion was invoked within.
        /// </summary>
        public Document Document { get; }

        /// <summary>
        /// The caret position when completion was triggered.
        /// </summary>
        public int Position { get; }

        /// <summary>
        /// The span of the document the completion list corresponds to.  It will be set initially to
        /// the result of <see cref="CompletionService.GetDefaultCompletionListSpan"/>, but it can
        /// be overwritten bduring <see cref="CompletionService.GetCompletionsAsync"/>.  The purpose
        /// of the span is to:
        ///     1. Signify where the completions should be presented.
        ///     2. Designate any existing text in the document that should be used for filtering.
        ///     3. Specify, by default, what portion of the text should be replaced when a completion 
        ///        item is committed.
        /// </summary>
        public TextSpan CompletionListSpan { get; set; }

        /// <summary>
        /// The triggering action that caused completion to be started.
        /// </summary>
        public CompletionTrigger Trigger { get; }

        /// <summary>
        /// The options that completion was started with.
        /// </summary>
        public OptionSet Options { get; }

        /// <summary>
        /// The cancellation token to use for this operation.
        /// </summary>
        public CancellationToken CancellationToken { get; }

        /// <summary>
        /// Set to true if the items added here should be the only items presented to the user.
        /// </summary>
        public bool IsExclusive { get; set; }

        /// <summary>
        /// Creates a <see cref="CompletionContext"/> instance.
        /// </summary>
        public CompletionContext(
            CompletionProvider provider,
            Document document,
            int position,
            TextSpan defaultSpan,
            CompletionTrigger trigger,
            OptionSet options,
            CancellationToken cancellationToken)
        {
            this.Provider = provider ?? throw new ArgumentNullException(nameof(provider));
            this.Document = document ?? throw new ArgumentNullException(nameof(document));
            this.Position = position;
            this.CompletionListSpan = defaultSpan;
            this.Trigger = trigger;
            this.Options = options ?? throw new ArgumentException(nameof(options));
            this.CancellationToken = cancellationToken;
            _items = new List<CompletionItem>();
        }

        public void AddItem(CompletionItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            _items.Add(item);
        }

        public void AddItems(IEnumerable<CompletionItem> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            foreach (var item in items)
            {
                AddItem(item);
            }
        }

        /// <summary>
        /// An optional <see cref="CompletionItem"/> that appears selected in the list presented to the user during suggestion mode.
        /// 
        /// Suggestion mode disables autoselection of items in the list, giving preference to the text typed by the user unless a specific item is selected manually.
        /// 
        /// Specifying a <see cref="SuggestionModeItem"/> is a request that the completion host operate in suggestion mode.
        /// The item specified determines the text displayed and the description associated with it unless a different item is manually selected.
        /// 
        /// No text is ever inserted when this item is completed, leaving the text the user typed instead.
        /// </summary>
        public CompletionItem SuggestionModeItem { get; set; }
    }
}