// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using ShaderTools.CodeAnalysis.Symbols;
using TaggedText = Microsoft.CodeAnalysis.TaggedText;

namespace ShaderTools.CodeAnalysis.SignatureHelp
{
    internal abstract partial class AbstractSignatureHelpProvider
    {
        internal class SymbolSignatureHelpItem : SignatureHelpItem, IEquatable<SymbolSignatureHelpItem>
        {
            public ISymbol Symbol { get; }

            public SymbolSignatureHelpItem(
                ISymbol symbol,
                bool isVariadic,
                Func<CancellationToken, IEnumerable<TaggedText>> documentationFactory,
                IEnumerable<TaggedText> prefixParts,
                IEnumerable<TaggedText> separatorParts,
                IEnumerable<TaggedText> suffixParts,
                IEnumerable<SignatureHelpParameter> parameters,
                IEnumerable<TaggedText> descriptionParts) :
                base(isVariadic, documentationFactory, prefixParts, separatorParts, suffixParts, parameters, descriptionParts)
            {
                Symbol = symbol;
            }

            public override bool Equals(object obj)
            {
                return this.Equals(obj as SymbolSignatureHelpItem);
            }

            public bool Equals(SymbolSignatureHelpItem obj)
            {
                return ReferenceEquals(this, obj) ||
                       (obj?.Symbol != null &&
                        this.Symbol != null &&
                        this.Symbol.Equals(obj.Symbol));
            }

            public override int GetHashCode()
            {
                if (this.Symbol == null)
                {
                    return 0;
                }

                return Symbol.GetHashCode();
            }
        }
    }
}