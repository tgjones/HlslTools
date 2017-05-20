// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Windows;
using ShaderTools.CodeAnalysis.Editor.Shared.Extensions;
using ShaderTools.CodeAnalysis.Editor.Shared.Utilities;
using ShaderTools.CodeAnalysis.Symbols.Markup;

namespace ShaderTools.CodeAnalysis.Editor.Implementation.IntelliSense.QuickInfo
{
    internal class ClassifiableDeferredContent : IDeferredQuickInfoContent
    {
        // Internal for testing purposes.
        internal readonly IList<TaggedText> ClassifiableContent;
        private readonly ClassificationTypeMap _typeMap;
        private readonly ITaggedTextMappingService _mappingService;

        public ClassifiableDeferredContent(
            IList<TaggedText> content,
            ClassificationTypeMap typeMap,
            ITaggedTextMappingService mappingService)
        {
            this.ClassifiableContent = content;
            _typeMap = typeMap;
            _mappingService = mappingService;
        }

        public virtual FrameworkElement Create()
        {
            var classifiedTextBlock = ClassifiableContent.ToTextBlock(_typeMap, _mappingService);

            if (classifiedTextBlock.Inlines.Count == 0)
            {
                classifiedTextBlock.Visibility = Visibility.Collapsed;
            }

            return classifiedTextBlock;
        }
    }
}