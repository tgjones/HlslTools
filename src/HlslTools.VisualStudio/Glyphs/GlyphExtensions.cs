using System;
using System.Drawing;
using System.Windows.Media;
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
    }
}