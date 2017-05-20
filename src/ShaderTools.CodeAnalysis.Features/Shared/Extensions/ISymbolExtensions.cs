// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Threading;
using ShaderTools.CodeAnalysis.Symbols;

namespace ShaderTools.CodeAnalysis.Shared.Extensions
{
    internal static partial class ISymbolExtensions2
    {
        public static Glyph GetGlyph(this ISymbol symbol)
        {
            switch (symbol.Kind)
            {
                case SymbolKind.Array:
                    return Glyph.ClassPublic;
                case SymbolKind.Namespace:
                    return Glyph.Namespace;
                case SymbolKind.Struct:
                    return Glyph.StructurePublic;
                case SymbolKind.Class:
                    return Glyph.ClassPublic;
                case SymbolKind.Interface:
                    return Glyph.InterfacePublic;
                case SymbolKind.Field:
                    return Glyph.FieldPublic;
                case SymbolKind.Function:
                    return Glyph.MethodPublic;
                case SymbolKind.Variable:
                    return Glyph.Local; // Not quite right.
                case SymbolKind.Parameter:
                    return Glyph.Parameter;
                case SymbolKind.Indexer:
                    return Glyph.MethodPublic;
                case SymbolKind.IntrinsicObjectType:
                    return Glyph.Intrinsic;
                case SymbolKind.IntrinsicVectorType:
                    return Glyph.Intrinsic;
                case SymbolKind.IntrinsicMatrixType:
                    return Glyph.Intrinsic;
                case SymbolKind.IntrinsicScalarType:
                    return Glyph.Intrinsic;
                case SymbolKind.Semantic:
                    return Glyph.ConstantPublic;
                case SymbolKind.Technique:
                    return Glyph.ModulePublic;
                case SymbolKind.Attribute:
                    return Glyph.MethodPublic;
                case SymbolKind.ConstantBuffer:
                    return Glyph.StructurePublic;
                case SymbolKind.TypeAlias:
                    return Glyph.Typedef;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static string GetDocumentation(ISymbol symbol, CancellationToken cancellationToken)
        {
            return symbol.Documentation;
        }
    }
}