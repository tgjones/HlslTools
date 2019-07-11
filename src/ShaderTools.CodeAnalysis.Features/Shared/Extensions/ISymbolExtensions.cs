// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using ShaderTools.CodeAnalysis.Compilation;
using ShaderTools.CodeAnalysis.Symbols;
using ShaderTools.Utilities.Collections;
using TaggedText = Microsoft.CodeAnalysis.TaggedText;

namespace ShaderTools.CodeAnalysis.Shared.Extensions
{
    internal static partial class ISymbolExtensions2
    {
        public static Glyph GetGlyph(this ISymbol symbol)
        {
            switch (symbol.Kind)
            {
                case SymbolKind.Array:
                    return Glyph.Class;
                case SymbolKind.Namespace:
                    return Glyph.Namespace;
                case SymbolKind.Struct:
                    return Glyph.Structure;
                case SymbolKind.Class:
                    return Glyph.Class;
                case SymbolKind.Interface:
                    return Glyph.Interface;
                case SymbolKind.Field:
                    return Glyph.Field;
                case SymbolKind.Function:
                    return Glyph.Method;
                case SymbolKind.Variable:
                    return Glyph.Local; // Not quite right.
                case SymbolKind.Parameter:
                    return Glyph.Parameter;
                case SymbolKind.Indexer:
                    return Glyph.Method;
                case SymbolKind.IntrinsicObjectType:
                    return Glyph.IntrinsicClass;
                case SymbolKind.IntrinsicVectorType:
                    return Glyph.IntrinsicStruct;
                case SymbolKind.IntrinsicMatrixType:
                    return Glyph.IntrinsicStruct;
                case SymbolKind.IntrinsicScalarType:
                    return Glyph.IntrinsicStruct;
                case SymbolKind.Semantic:
                    return Glyph.Constant;
                case SymbolKind.Technique:
                    return Glyph.Module;
                case SymbolKind.Attribute:
                    return Glyph.Method;
                case SymbolKind.ConstantBuffer:
                    return Glyph.Structure;
                case SymbolKind.TypeAlias:
                    return Glyph.Typedef;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static string GetFullyQualifiedName(this ISymbol symbol)
        {
            var result = new StringBuilder();
            GetFullyQualifiedNameRecursive(symbol, result);
            return result.ToString();
        }

        private static void GetFullyQualifiedNameRecursive(this ISymbol symbol, StringBuilder sb)
        {
            if (symbol.Parent != null)
            {
                GetFullyQualifiedNameRecursive(symbol.Parent, sb);
                sb.Append("::");
            }

            sb.Append(symbol.Name);
        }

        private static string GetDocumentation(ISymbol symbol, CancellationToken cancellationToken)
        {
            return symbol.Documentation;
        }

        public static IEnumerable<TaggedText> GetDocumentationParts(this ISymbol symbol, SemanticModelBase semanticModel, int position, CancellationToken cancellationToken)
        {
            string documentation = GetDocumentation(symbol, cancellationToken);

            return documentation != null
                ? SpecializedCollections.SingletonEnumerable(new TaggedText(TextTags.Text, documentation))
                : SpecializedCollections.EmptyEnumerable<TaggedText>();
        }

        public static string ToDisplayString(this ISymbol symbol, SymbolDisplayFormat format)
        {
            return symbol.ToMarkup(format).Tokens.ToTaggedText().GetFullText();
        }
    }
}