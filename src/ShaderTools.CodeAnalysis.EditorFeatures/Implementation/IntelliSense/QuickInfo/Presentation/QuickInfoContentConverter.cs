using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows;
using Microsoft.VisualStudio.Language.Intellisense;
using ShaderTools.CodeAnalysis.Editor.Shared.Utilities;
using ShaderTools.CodeAnalysis.QuickInfo;
using ShaderTools.Utilities.Collections;

namespace ShaderTools.CodeAnalysis.Editor.Implementation.IntelliSense.QuickInfo.Presentation
{
    [Export]
    internal sealed class QuickInfoContentConverter
    {
        private readonly IGlyphService _glyphService;
        private readonly ClassificationTypeMap _classificationTypeMap;

        [ImportingConstructor]
        public QuickInfoContentConverter(
            IGlyphService glyphService,
            ClassificationTypeMap classificationTypeMap)
        {
            _glyphService = glyphService;
            _classificationTypeMap = classificationTypeMap;
        }

        public FrameworkElement CreateFrameworkElement(QuickInfoContent content)
        {
            return CreateDeferredContent(content).Create();
        }

        private IDeferredQuickInfoContent CreateDeferredContent(QuickInfoContent content)
        {
            switch (content)
            {
                case QuickInfoDisplayContent c:
                    var taggedTextMappingService = PrimaryWorkspace.Workspace.Services.GetLanguageServices(c.Language).GetService<ITaggedTextMappingService>();

                    return new QuickInfoDisplayDeferredContent(
                        new SymbolGlyphDeferredContent(c.Glyph, _glyphService),
                        null,
                        new ClassifiableDeferredContent(c.MainDescription, _classificationTypeMap, taggedTextMappingService),
                        new ClassifiableDeferredContent(c.Documentation, _classificationTypeMap, taggedTextMappingService),
                        new ClassifiableDeferredContent(SpecializedCollections.EmptyList<TaggedText>(), _classificationTypeMap, taggedTextMappingService),
                        new ClassifiableDeferredContent(SpecializedCollections.EmptyList<TaggedText>(), _classificationTypeMap, taggedTextMappingService),
                        new ClassifiableDeferredContent(SpecializedCollections.EmptyList<TaggedText>(), _classificationTypeMap, taggedTextMappingService),
                        new ClassifiableDeferredContent(SpecializedCollections.EmptyList<TaggedText>(), _classificationTypeMap, taggedTextMappingService));

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
