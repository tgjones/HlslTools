// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using ShaderTools.CodeAnalysis.Symbols;

namespace ShaderTools.CodeAnalysis.Shared.Extensions
{
    // Note - these methods are called in fairly hot paths in the IDE, so we try to be responsible about allocations.
    internal static class SymbolInfoExtensions
    {
        public static ImmutableArray<ISymbol> GetBestOrAllSymbols(this SymbolInfo info)
        {
            if (info.Symbol != null)
            {
                return ImmutableArray.Create(info.Symbol);
            }
            else if (info.CandidateSymbols.Length > 0)
            {
                return info.CandidateSymbols;
            }

            return ImmutableArray<ISymbol>.Empty;
        }
    }
}
