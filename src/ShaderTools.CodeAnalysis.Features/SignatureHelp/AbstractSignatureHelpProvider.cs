// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ShaderTools.CodeAnalysis.Shared.Extensions;
using ShaderTools.CodeAnalysis.Symbols;
using ShaderTools.CodeAnalysis.Symbols.Markup;
using ShaderTools.CodeAnalysis.Syntax;
using ShaderTools.CodeAnalysis.Text;
using ShaderTools.Utilities.Collections;

namespace ShaderTools.CodeAnalysis.SignatureHelp
{
    internal abstract partial class AbstractSignatureHelpProvider : ISignatureHelpProvider
    {
        protected AbstractSignatureHelpProvider()
        {
        }

        public abstract bool IsTriggerCharacter(char ch);
        public abstract bool IsRetriggerCharacter(char ch);

        protected abstract Task<SignatureHelpItems> GetItemsWorkerAsync(Document document, int position, SignatureHelpTriggerInfo triggerInfo, CancellationToken cancellationToken);

        protected static SignatureHelpItems CreateSignatureHelpItems(
            IList<SignatureHelpItem> items, SourceFileSpan applicableSpan, SignatureHelpState state, int? selectedItem)
        {
            if (items == null || !items.Any() || state == null)
            {
                return null;
            }

            if (!applicableSpan.IsInRootFile)
            {
                return null;
            }

            items = Filter(items, state.ArgumentNames);
            return new SignatureHelpItems(items, applicableSpan.Span, state.ArgumentIndex, state.ArgumentCount, state.ArgumentName, selectedItem);
        }

        private static IList<SignatureHelpItem> Filter(IList<SignatureHelpItem> items, IEnumerable<string> parameterNames)
        {
            if (parameterNames == null)
            {
                return items.ToList();
            }

            var filteredList = items.Where(i => Include(i, parameterNames)).ToList();
            return filteredList.Count == 0 ? items.ToList() : filteredList;
        }

        private static bool Include(SignatureHelpItem item, IEnumerable<string> parameterNames)
        {
            var itemParameterNames = item.Parameters.Select(p => p.Name).ToSet();
            return parameterNames.All(itemParameterNames.Contains);
        }

        //public abstract Task<SignatureHelpState> GetCurrentArgumentStateAsync(Document document, int position, TextSpan currentSpan, CancellationToken cancellationToken);

        protected SignatureHelpItem CreateItem(
            ISymbol orderSymbol,
            bool isVariadic,
            Func<CancellationToken, IEnumerable<TaggedText>> documentationFactory,
            IList<SymbolMarkupToken> prefixParts,
            IList<SymbolMarkupToken> separatorParts,
            IList<SymbolMarkupToken> suffixParts,
            IList<SignatureHelpSymbolParameter> parameters,
            IList<SymbolMarkupToken> descriptionParts = null)
        {
            descriptionParts = descriptionParts == null
                ? SpecializedCollections.EmptyList<SymbolMarkupToken>()
                : descriptionParts;

            var allParts = prefixParts.Concat(separatorParts)
                .Concat(suffixParts)
                .Concat(parameters.SelectMany(p => p.GetAllParts()))
                .Concat(descriptionParts);

            return new SymbolSignatureHelpItem(
                orderSymbol,
                isVariadic,
                documentationFactory,
                prefixParts.ToTaggedText(),
                separatorParts.ToTaggedText(),
                suffixParts.ToTaggedText(),
                parameters.Select(p => (SignatureHelpParameter) p),
                descriptionParts.ToTaggedText());
        }

        public async Task<SignatureHelpItems> GetItemsAsync(
            Document document, int position, SignatureHelpTriggerInfo triggerInfo, CancellationToken cancellationToken)
        {
            var itemsForCurrentDocument = await GetItemsWorkerAsync(document, position, triggerInfo, cancellationToken).ConfigureAwait(false);
            return itemsForCurrentDocument;
        }
    }
}
