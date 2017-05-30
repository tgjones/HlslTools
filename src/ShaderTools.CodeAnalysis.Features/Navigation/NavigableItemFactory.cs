// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using ShaderTools.CodeAnalysis.Symbols;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.Navigation
{
    internal static partial class NavigableItemFactory
    {
        public static INavigableItem GetItemFromDeclaredSymbol(ISymbol declaredSymbol, Document document, SourceFileSpan sourceFileSpan)
        {
            return new DeclaredSymbolNavigableItem(document, declaredSymbol, sourceFileSpan);
        }
    }
}