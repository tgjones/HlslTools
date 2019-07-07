// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Windows.Media;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Language.Intellisense;

namespace ShaderTools.CodeAnalysis.Editor.Shared.Extensions
{
    internal static class GlyphExtensions
    {
        public static StandardGlyphGroup GetStandardGlyphGroup(this Glyph glyph)
        {
            switch (glyph)
            {
                case Glyph.Class:
                    return StandardGlyphGroup.GlyphGroupClass;

                case Glyph.Constant:
                    return StandardGlyphGroup.GlyphGroupConstant;

                case Glyph.Field:
                    return StandardGlyphGroup.GlyphGroupField;

                case Glyph.Interface:
                    return StandardGlyphGroup.GlyphGroupInterface;

                case Glyph.IntrinsicClass:
                case Glyph.IntrinsicStruct:
                    return StandardGlyphGroup.GlyphGroupIntrinsic;

                case Glyph.Keyword:
                    return StandardGlyphGroup.GlyphKeyword;

                case Glyph.Label:
                    return StandardGlyphGroup.GlyphGroupIntrinsic;

                case Glyph.Local:
                    return StandardGlyphGroup.GlyphGroupVariable;

                case Glyph.Macro:
                    return StandardGlyphGroup.GlyphGroupMacro;

                case Glyph.Namespace:
                    return StandardGlyphGroup.GlyphGroupNamespace;

                case Glyph.Method:
                    return StandardGlyphGroup.GlyphGroupMethod;

                case Glyph.Module:
                    return StandardGlyphGroup.GlyphGroupModule;

                case Glyph.OpenFolder:
                    return StandardGlyphGroup.GlyphOpenFolder;

                case Glyph.Operator:
                    return StandardGlyphGroup.GlyphGroupOperator;

                case Glyph.Parameter:
                    return StandardGlyphGroup.GlyphGroupVariable;

                case Glyph.Structure:
                    return StandardGlyphGroup.GlyphGroupStruct;

                case Glyph.Typedef:
                    return StandardGlyphGroup.GlyphGroupTypedef;

                case Glyph.TypeParameter:
                    return StandardGlyphGroup.GlyphGroupType;

                case Glyph.CompletionWarning:
                    return StandardGlyphGroup.GlyphCompletionWarning;

                default:
                    throw new ArgumentException("glyph");
            }
        }

        public static StandardGlyphItem GetStandardGlyphItem(this Glyph icon)
        {
            return StandardGlyphItem.GlyphItemPublic;
        }

        public static ImageSource GetImageSource(this Glyph? glyph, IGlyphService glyphService)
        {
            return glyph.HasValue ? glyph.Value.GetImageSource(glyphService) : null;
        }

        public static ImageSource GetImageSource(this Glyph glyph, IGlyphService glyphService)
        {
            return glyphService.GetGlyph(glyph.GetStandardGlyphGroup(), glyph.GetStandardGlyphItem());
        }

        public static ImageMoniker GetImageMoniker(this Glyph glyph)
        {
            switch (glyph)
            {
                case Glyph.None:
                    return default(ImageMoniker);

                case Glyph.Class:
                    return KnownMonikers.ClassPublic;

                case Glyph.Constant:
                    return KnownMonikers.ConstantPublic;

                case Glyph.Field:
                    return KnownMonikers.FieldPublic;

                case Glyph.Interface:
                    return KnownMonikers.InterfacePublic;

                // TODO: Figure out the right thing to return here.
                case Glyph.IntrinsicClass:
                case Glyph.IntrinsicStruct:
                    return KnownMonikers.Type;

                case Glyph.Keyword:
                    return KnownMonikers.IntellisenseKeyword;

                case Glyph.Label:
                    return KnownMonikers.Label;

                case Glyph.Macro:
                    return KnownMonikers.MacroPublic;

                case Glyph.Parameter:
                case Glyph.Local:
                    return KnownMonikers.LocalVariable;

                case Glyph.Namespace:
                    return KnownMonikers.Namespace;

                case Glyph.Method:
                    return KnownMonikers.MethodPublic;

                case Glyph.Module:
                    return KnownMonikers.ModulePublic;

                case Glyph.OpenFolder:
                    return KnownMonikers.OpenFolder;

                case Glyph.Operator:
                    return KnownMonikers.Operator;

                case Glyph.Structure:
                    return KnownMonikers.ValueTypePublic;

                case Glyph.Typedef:
                    return KnownMonikers.TypeDefinitionPublic;

                case Glyph.TypeParameter:
                    return KnownMonikers.Type;

                case Glyph.CompletionWarning:
                    return KnownMonikers.IntellisenseWarning;

                default:
                    throw new ArgumentException("glyph");
            }
        }
    }
}