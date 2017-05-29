// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.QuickInfo
{
    internal class QuickInfoItem
    {
        public TextSpan TextSpan { get; }
        public QuickInfoContent Content { get; }

        public QuickInfoItem(TextSpan textSpan, QuickInfoContent content)
        {
            this.TextSpan = textSpan;
            this.Content = content;
        }
    }

    internal abstract class QuickInfoContent
    {
        
    }

    internal sealed class QuickInfoDisplayContent : QuickInfoContent
    {
        public string Language { get; }

        public Glyph Glyph { get; }

        public ImmutableArray<TaggedText> MainDescription { get; }

        public ImmutableArray<TaggedText> Documentation { get; }

        public QuickInfoDisplayContent(string language, Glyph glyph, ImmutableArray<TaggedText> mainDescription, ImmutableArray<TaggedText> documentation)
        {
            Language = language;
            Glyph = glyph;
            MainDescription = mainDescription;
            Documentation = documentation;
        }
    }

    internal sealed class QuickInfoElidedContent : QuickInfoContent
    {
        // TODO
    }
}
