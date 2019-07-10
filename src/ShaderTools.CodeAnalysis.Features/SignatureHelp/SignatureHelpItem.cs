// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using ShaderTools.CodeAnalysis.Symbols.Markup;
using ShaderTools.Utilities.Collections;
using TaggedText = Microsoft.CodeAnalysis.TaggedText;

namespace ShaderTools.CodeAnalysis.SignatureHelp
{
    internal class SignatureHelpItem
    {
        /// <summary>
        /// True if this signature help item can have an unbounded number of arguments passed to it.
        /// If it is variadic then the last parameter will be considered selected, even if the
        /// selected parameter index strictly goes past the number of defined parameters for this
        /// item.
        /// </summary>
        public bool IsVariadic { get; }

        public ImmutableArray<TaggedText> PrefixDisplayParts { get; }
        public ImmutableArray<TaggedText> SuffixDisplayParts { get; }

        // TODO: This probably won't be sufficient for VB query signature help.  It has
        // arbitrary separators between parameters.
        public ImmutableArray<TaggedText> SeparatorDisplayParts { get; }

        public ImmutableArray<SignatureHelpParameter> Parameters { get; }

        public ImmutableArray<TaggedText> DescriptionParts { get; internal set; }

        public Func<CancellationToken, IEnumerable<TaggedText>> DocumentationFactory { get; }

        private static readonly Func<CancellationToken, IEnumerable<TaggedText>> s_emptyDocumentationFactory =
            _ => SpecializedCollections.EmptyEnumerable<TaggedText>();

        public SignatureHelpItem(
            bool isVariadic,
            Func<CancellationToken, IEnumerable<TaggedText>> documentationFactory,
            IEnumerable<TaggedText> prefixParts,
            IEnumerable<TaggedText> separatorParts,
            IEnumerable<TaggedText> suffixParts,
            IEnumerable<SignatureHelpParameter> parameters,
            IEnumerable<TaggedText> descriptionParts)
        {
            if (isVariadic && !parameters.Any())
            {
                throw new ArgumentException("Variadic SignatureHelpItem must have at least one parameter.");
            }

            this.IsVariadic = isVariadic;
            this.DocumentationFactory = documentationFactory ?? s_emptyDocumentationFactory;
            this.PrefixDisplayParts = prefixParts.ToImmutableArrayOrEmpty();
            this.SeparatorDisplayParts = separatorParts.ToImmutableArrayOrEmpty();
            this.SuffixDisplayParts = suffixParts.ToImmutableArrayOrEmpty();
            this.Parameters = parameters.ToImmutableArrayOrEmpty();
            this.DescriptionParts = descriptionParts.ToImmutableArrayOrEmpty();
        }

        // Constructor kept for back compat
        public SignatureHelpItem(
            bool isVariadic,
            Func<CancellationToken, IEnumerable<SymbolMarkupToken>> documentationFactory,
            IEnumerable<SymbolMarkupToken> prefixParts,
            IEnumerable<SymbolMarkupToken> separatorParts,
            IEnumerable<SymbolMarkupToken> suffixParts,
            IEnumerable<SignatureHelpParameter> parameters,
            IEnumerable<SymbolMarkupToken> descriptionParts)
            : this(isVariadic,
                documentationFactory != null
                    ? c => documentationFactory(c).ToTaggedText()
                    : s_emptyDocumentationFactory,
                prefixParts.ToTaggedText(),
                separatorParts.ToTaggedText(),
                suffixParts.ToTaggedText(),
                parameters,
                descriptionParts.ToTaggedText())
        {
        }

        internal IEnumerable<TaggedText> GetAllParts()
        {
            return
                PrefixDisplayParts.Concat(
                    Enumerable.Concat(SeparatorDisplayParts, Enumerable.Concat(SuffixDisplayParts, Parameters.SelectMany(p => p.GetAllParts())).Concat(
                            DescriptionParts)));
        }
    }
}
