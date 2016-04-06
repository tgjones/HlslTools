using System;
using System.Drawing;
using System.Windows.Media;
using HlslTools.Symbols;
using Microsoft.VisualStudio.Language.Intellisense;

namespace HlslTools.VisualStudio.Glyphs
{
    internal static class GlyphExtensions
    {
        public static StandardGlyphGroup GetStandardGlyphGroup(this Glyph glyph)
        {
            switch (glyph)
            {
                case Glyph.TopLevel:
                case Glyph.Namespace:
                    return StandardGlyphGroup.GlyphGroupNamespace;
                case Glyph.Class:
                    return StandardGlyphGroup.GlyphGroupClass;
                case Glyph.Interface:
                    return StandardGlyphGroup.GlyphGroupInterface;
                case Glyph.Struct:
                case Glyph.ConstantBuffer:
                    return StandardGlyphGroup.GlyphGroupStruct;
                case Glyph.Technique:
                    return StandardGlyphGroup.GlyphGroupModule;
                case Glyph.Function:
                    return StandardGlyphGroup.GlyphGroupMethod;
                case Glyph.Variable:
                case Glyph.Parameter:
                    return StandardGlyphGroup.GlyphGroupVariable;
                case Glyph.Keyword:
                    return StandardGlyphGroup.GlyphKeyword;
                case Glyph.Semantic:
                    return StandardGlyphGroup.GlyphGroupConstant;
                case Glyph.Field:
                    return StandardGlyphGroup.GlyphGroupField;
                case Glyph.Method:
                    return StandardGlyphGroup.GlyphGroupMethod;
                case Glyph.Intrinsic:
                    return StandardGlyphGroup.GlyphGroupIntrinsic;
                case Glyph.Macro:
                    return StandardGlyphGroup.GlyphGroupMacro;
                case Glyph.CompletionWarning:
                    return StandardGlyphGroup.GlyphCompletionWarning;
                default:
                    throw new ArgumentOutOfRangeException(nameof(glyph), glyph, null);
            }
        }

        public static ImageSource GetImageSource(this Glyph glyph, DispatcherGlyphService glyphService)
        {
            return glyphService.GetGlyph(glyph.GetStandardGlyphGroup(), StandardGlyphItem.GlyphItemPublic);
        }

        public static Icon GetIcon(this Glyph glyph, DispatcherGlyphService glyphService)
        {
            return glyphService.GetIcon(glyph.GetStandardGlyphGroup(), StandardGlyphItem.GlyphItemPublic);
        }

        public static Glyph GetGlyph(this Symbol symbol)
        {
            switch (symbol.Kind)
            {
                case SymbolKind.Array:
                    return Glyph.Class;
                case SymbolKind.Namespace:
                    return Glyph.Namespace;
                case SymbolKind.Struct:
                    return Glyph.Struct;
                case SymbolKind.Class:
                    return Glyph.Class;
                case SymbolKind.Interface:
                    return Glyph.Interface;
                case SymbolKind.Field:
                    return Glyph.Field;
                case SymbolKind.Function:
                    return symbol.Parent is TypeSymbol ? Glyph.Method : Glyph.Function;
                case SymbolKind.Variable:
                    return Glyph.Variable;
                case SymbolKind.Parameter:
                    return Glyph.Parameter;
                case SymbolKind.Indexer:
                    return Glyph.Function;
                case SymbolKind.IntrinsicObjectType:
                    return Glyph.Intrinsic;
                case SymbolKind.IntrinsicVectorType:
                    return Glyph.Intrinsic;
                case SymbolKind.IntrinsicMatrixType:
                    return Glyph.Intrinsic;
                case SymbolKind.IntrinsicScalarType:
                    return Glyph.Intrinsic;
                case SymbolKind.Semantic:
                    return Glyph.Semantic;
                case SymbolKind.Technique:
                    return Glyph.Technique;
                case SymbolKind.Attribute:
                    return Glyph.Function;
                case SymbolKind.ConstantBuffer:
                    return Glyph.ConstantBuffer;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}