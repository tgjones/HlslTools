// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Projection;
using ShaderTools.CodeAnalysis.Editor.Implementation.IntelliSense.QuickInfo;
using ShaderTools.CodeAnalysis.Editor.Shared.Utilities;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Syntax;
using ShaderTools.Utilities.Collections;

namespace ShaderTools.CodeAnalysis.Editor.Hlsl.QuickInfo
{
    [ExportQuickInfoProvider(PredefinedQuickInfoProviderNames.Syntactic, LanguageNames.Hlsl)]
    internal class HlslSyntacticQuickInfoProvider : AbstractQuickInfoProvider
    {
        [ImportingConstructor]
        public HlslSyntacticQuickInfoProvider(
            IProjectionBufferFactoryService projectionBufferFactoryService,
            IEditorOptionsFactoryService editorOptionsFactoryService,
            ITextEditorFactoryService textEditorFactoryService,
            IGlyphService glyphService,
            ClassificationTypeMap typeMap)
            : base(projectionBufferFactoryService, editorOptionsFactoryService,
                   textEditorFactoryService, glyphService, typeMap,
                   PrimaryWorkspace.Workspace.Services.GetLanguageServices(LanguageNames.Hlsl).GetRequiredService<ITaggedTextMappingService>())
        {
        }

        protected override async Task<IDeferredQuickInfoContent> BuildContentAsync(Document document, ISyntaxToken token, CancellationToken cancellationToken)
        {
            var macroDefinitionNode = token.Parent as DefineDirectiveTriviaSyntax;
            if (macroDefinitionNode != null && macroDefinitionNode.MacroName == token)
            {
                return CreateQuickInfoDisplayDeferredContent(
                    Glyph.Macro,
                    new List<TaggedText> { new TaggedText(TextTags.Text, $"(macro definition) {macroDefinitionNode}") },
                    CreateDocumentationCommentDeferredContent(null),
                    SpecializedCollections.EmptyList<TaggedText>(),
                    SpecializedCollections.EmptyList<TaggedText>(),
                    SpecializedCollections.EmptyList<TaggedText>(),
                    SpecializedCollections.EmptyList<TaggedText>());
            }

            var syntaxToken = (SyntaxToken) token;
            var macroReference = syntaxToken.MacroReference;
            if (macroReference != null && macroReference.NameToken.SourceRange == token.SourceRange)
            {
                return CreateQuickInfoDisplayDeferredContent(
                    Glyph.Macro,
                    new List<TaggedText> { new TaggedText(TextTags.Text, $"(macro reference) {macroReference.DefineDirective.ToString(true)}") },
                    CreateDocumentationCommentDeferredContent(null),
                    SpecializedCollections.EmptyList<TaggedText>(),
                    SpecializedCollections.EmptyList<TaggedText>(),
                    SpecializedCollections.EmptyList<TaggedText>(),
                    SpecializedCollections.EmptyList<TaggedText>());
            }

            return null;
        }
    }
}
