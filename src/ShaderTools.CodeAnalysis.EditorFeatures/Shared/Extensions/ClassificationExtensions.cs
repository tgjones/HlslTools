// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Classification;
using ShaderTools.CodeAnalysis.Classification;
using ShaderTools.CodeAnalysis.Editor.Shared.Utilities;
using TaggedText = Microsoft.CodeAnalysis.TaggedText;

namespace ShaderTools.CodeAnalysis.Editor.Shared.Extensions
{
    internal static partial class ClassificationExtensions
    {
        public static IList<ClassificationSpan> ToClassificationSpans(
            this IEnumerable<TaggedText> parts,
            ITextSnapshot textSnapshot,
            ClassificationTypeMap typeMap,
            ITaggedTextMappingService mappingService)
        {
            var result = new List<ClassificationSpan>();

            var index = 0;
            foreach (var part in parts)
            {
                var text = part.ToString();
                result.Add(new ClassificationSpan(
                    new SnapshotSpan(textSnapshot, new Microsoft.VisualStudio.Text.Span(index, text.Length)),
                    typeMap.GetClassificationType(part.Tag.ToClassificationTypeName(mappingService))));

                index += text.Length;
            }

            return result;
        }

        public static ClassifiedTextElement ToClassifiedTextElement(
            this IEnumerable<TaggedText> parts,
            ITaggedTextMappingService mappingService)
        {
            var inlines = parts.ToClassifiedTextRuns(mappingService);
            return inlines.ToClassifiedTextElement();
        }

        public static IList<ClassifiedTextRun> ToClassifiedTextRuns(
            this IEnumerable<TaggedText> parts,
            ITaggedTextMappingService mappingService)
        {
            var classifiedTexts = parts.Select(p =>
                new ClassifiedText(
                    p.Tag.ToClassificationTypeName(mappingService),
                    p.ToVisibleDisplayString(includeLeftToRightMarker: true)));
            return classifiedTexts.ToClassifiedTextRuns();
        }

        public static IList<ClassifiedTextRun> ToClassifiedTextRuns(
           this IEnumerable<ClassifiedText> parts)
        {
            var inlines = new List<ClassifiedTextRun>();

            foreach (var part in parts)
            {
                inlines.Add(part.ToClassifiedTextRun());
            }

            return inlines;
        }

        public static ClassifiedTextRun ToClassifiedTextRun(this ClassifiedText part)
        {
            return new ClassifiedTextRun(part.ClassificationType, part.Text);
        }

        public static ClassifiedTextElement ToClassifiedTextElement(this IEnumerable<ClassifiedTextRun> textRuns)
        {
            return new ClassifiedTextElement(textRuns);
        }
    }
}