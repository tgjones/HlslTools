// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Text;
using ShaderTools.Utilities.Collections;

namespace ShaderTools.CodeAnalysis.Completion
{
    /// <summary>
    /// One of many possible completions used to form the completion list presented to the user.
    /// </summary>
    [DebuggerDisplay("{DisplayText}")]
    internal sealed class CompletionItem : IComparable<CompletionItem>
    {
        /// <summary>
        /// The text that is displayed to the user.
        /// </summary>
        public string DisplayText { get; }

        /// <summary>
        /// The text used to determine if the item matches the filter and is show in the list.
        /// This is often the same as <see cref="DisplayText"/> but may be different in certain circumstances.
        /// </summary>
        public string FilterText { get; }

        /// <summary>
        /// The text used to determine the order that the item appears in the list.
        /// This is often the same as the <see cref="DisplayText"/> but may be different in certain circumstances.
        /// </summary>
        public string SortText { get; }

        /// <summary>
        /// The span of the syntax element associated with this item.
        /// 
        /// The span identifies the text in the document that is used to filter the initial list presented to the user,
        /// and typically represents the region of the document that will be changed if this item is committed.
        /// </summary>
        public TextSpan Span { get; internal set; }

        /// <summary>
        /// Additional information attached to a completion item by it creator.
        /// </summary>
        public ImmutableDictionary<string, string> Properties { get; }

        public Glyph Glyph { get; }

        /// <summary>
        /// Descriptive tags from <see cref="CompletionTags"/>.
        /// These tags may influence how the item is displayed.
        /// </summary>
        public ImmutableArray<string> Tags { get; }

        /// <summary>
        /// Rules that declare how this item should behave.
        /// </summary>
        public CompletionItemRules Rules { get; }

        private CompletionItem(
            string displayText,
            string filterText,
            string sortText,
            TextSpan span,
            ImmutableDictionary<string, string> properties,
            Glyph glyph,
            ImmutableArray<string> tags)
        {
            this.DisplayText = displayText ?? "";
            this.FilterText = filterText ?? this.DisplayText;
            this.SortText = sortText ?? this.DisplayText;
            this.Span = span;
            this.Properties = properties ?? ImmutableDictionary<string, string>.Empty;
            Glyph = glyph;
            this.Tags = tags.NullToEmpty();
            this.Rules = CompletionItemRules.Default;
        }

#pragma warning disable RS0027 // Public API with optional parameter(s) should have the most parameters amongst its public overloads.
        internal static CompletionItem Create(
#pragma warning restore RS0027 // Public API with optional parameter(s) should have the most parameters amongst its public overloads.
            string displayText,
            string filterText = null,
            string sortText = null,
            ImmutableDictionary<string, string> properties = null,
            Glyph glyph = Glyph.None,
            ImmutableArray<string> tags = default)
        {
            return new CompletionItem(
                span: default(TextSpan),
                displayText: displayText,
                filterText: filterText,
                sortText: sortText,
                properties: properties,
                glyph: glyph,
                tags: tags);
        }

        int IComparable<CompletionItem>.CompareTo(CompletionItem other)
        {
            var result = StringComparer.OrdinalIgnoreCase.Compare(this.SortText, other.SortText);
            if (result == 0)
            {
                result = StringComparer.OrdinalIgnoreCase.Compare(this.DisplayText, other.DisplayText);
            }

            return result;
        }

        public override string ToString() => DisplayText;
    }
}