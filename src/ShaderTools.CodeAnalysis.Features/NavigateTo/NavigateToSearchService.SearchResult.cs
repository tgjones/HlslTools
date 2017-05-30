// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using ShaderTools.CodeAnalysis.FindSymbols;
using ShaderTools.CodeAnalysis.Navigation;
using ShaderTools.CodeAnalysis.Shared.Extensions;
using ShaderTools.CodeAnalysis.Symbols;
using ShaderTools.CodeAnalysis.Text;
using ShaderTools.Utilities.PooledObjects;

namespace ShaderTools.CodeAnalysis.NavigateTo
{
    internal sealed partial class NavigateToSearchService
    {
        private class SearchResult : INavigateToSearchResult
        {
            public string AdditionalInformation
            {
                get
                {
                    if (_declaredSymbol.Parent == null)
                    {
                        return "(Global Scope)";
                    }

                    switch (_declaredSymbol.Parent.Kind)
                    {
                        case SymbolKind.Namespace:
                            return "namespace " + _declaredSymbol.Parent.GetFullyQualifiedName();

                        case SymbolKind.Class:
                            return "class " + _declaredSymbol.Parent.GetFullyQualifiedName();

                        case SymbolKind.Interface:
                            return "interface " + _declaredSymbol.Parent.GetFullyQualifiedName();

                        case SymbolKind.Struct:
                            return "struct " + _declaredSymbol.Parent.GetFullyQualifiedName();

                        case SymbolKind.ConstantBuffer:
                            return "constant buffer " + _declaredSymbol.Parent.GetFullyQualifiedName();

                        default:
                            return string.Empty;
                    }
                }
            }

            public string Name => _declaredSymbol.Name;
            public string Summary { get; }

            public string Kind { get; }
            public NavigateToMatchKind MatchKind { get; }
            public INavigableItem NavigableItem { get; }
            public string SecondarySort { get; }
            public bool IsCaseSensitive { get; }

            private readonly Document _document;
            private readonly ISymbol _declaredSymbol;

            public SearchResult(
                Document document, ISymbol declaredSymbol, string kind,
                NavigateToMatchKind matchKind, bool isCaseSensitive, INavigableItem navigableItem)
            {
                _document = document;
                _declaredSymbol = declaredSymbol;
                Kind = kind;
                MatchKind = matchKind;
                IsCaseSensitive = isCaseSensitive;
                NavigableItem = navigableItem;
                //SecondarySort = ConstructSecondarySortString(document, declaredSymbolInfo);
                SecondarySort = null;
            }

            private static readonly char[] s_dotArray = { '.' };

            //private static string ConstructSecondarySortString(
            //    Document document,
            //    DeclaredSymbolInfo declaredSymbolInfo)
            //{
            //    var parts = ArrayBuilder<string>.GetInstance();
            //    try
            //    {
            //        parts.Add(declaredSymbolInfo.ParameterCount.ToString("X4"));
            //        parts.Add(declaredSymbolInfo.TypeParameterCount.ToString("X4"));
            //        parts.Add(declaredSymbolInfo.Name);

            //        // For partial types, we break up the file name into pieces.  i.e. If we have
            //        // Outer.cs and Outer.Inner.cs  then we add "Outer" and "Outer Inner" to 
            //        // the secondary sort string.  That way "Outer.cs" will be weighted above
            //        // "Outer.Inner.cs"
            //        var fileName = Path.GetFileNameWithoutExtension(document.FilePath ?? "");
            //        parts.AddRange(fileName.Split(s_dotArray));

            //        return string.Join(" ", parts);
            //    }
            //    finally
            //    {
            //        parts.Free();
            //    }
            //}
        }
    }
}