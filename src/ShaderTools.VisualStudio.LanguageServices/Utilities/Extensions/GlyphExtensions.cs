// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.VisualStudio.Language.Intellisense;
using ShaderTools.CodeAnalysis;
using ShaderTools.CodeAnalysis.Editor.Shared.Extensions;

namespace ShaderTools.VisualStudio.LanguageServices.Utilities.Extensions
{
    internal static class GlyphExtensions
    {
        public static ushort GetGlyphIndex(this Glyph glyph)
        {
            var glyphGroup = glyph.GetStandardGlyphGroup();
            var glyphItem = glyph.GetStandardGlyphItem();

            return glyphGroup < StandardGlyphGroup.GlyphGroupError
                ? (ushort) ((int) glyphGroup + (int) glyphItem)
                : (ushort) glyphGroup;
        }
    }
}
