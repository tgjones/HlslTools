// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.QuickInfo
{
    internal sealed class QuickInfoItem
    {
        public TextSpan TextSpan { get; }
        public QuickInfoContent Content { get; }

        public QuickInfoItem(TextSpan textSpan, QuickInfoContent content)
        {
            TextSpan = textSpan;
            Content = content;
        }
    }

    internal sealed class QuickInfoContent
    {
        public string Language { get; }

        public Glyph Glyph { get; }

        public ImmutableArray<TaggedText> MainDescription { get; }

        public ImmutableArray<TaggedText> Documentation { get; }

        public QuickInfoContent(string language, Glyph glyph, ImmutableArray<TaggedText> mainDescription, ImmutableArray<TaggedText> documentation)
        {
            Language = language;
            Glyph = glyph;
            MainDescription = mainDescription;
            Documentation = documentation;
        }
    }
}
