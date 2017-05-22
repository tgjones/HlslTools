// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Projection;
using ShaderTools.CodeAnalysis.Compilation;
using ShaderTools.CodeAnalysis.Editor.Shared.Utilities;
using ShaderTools.CodeAnalysis.LanguageServices;
using ShaderTools.CodeAnalysis.Shared.Extensions;
using ShaderTools.CodeAnalysis.Symbols;
using ShaderTools.CodeAnalysis.Syntax;
using ShaderTools.Utilities.Collections;

namespace ShaderTools.CodeAnalysis.Editor.Implementation.IntelliSense.QuickInfo
{
    internal abstract partial class AbstractSemanticQuickInfoProvider : AbstractQuickInfoProvider
    {
        public AbstractSemanticQuickInfoProvider(
            IProjectionBufferFactoryService projectionBufferFactoryService,
            IEditorOptionsFactoryService editorOptionsFactoryService,
            ITextEditorFactoryService textEditorFactoryService,
            IGlyphService glyphService,
            ClassificationTypeMap typeMap,
            ITaggedTextMappingService taggedTextMappingService)
            : base(projectionBufferFactoryService, editorOptionsFactoryService,
                   textEditorFactoryService, glyphService, typeMap,
                   taggedTextMappingService)
        {
        }

        protected override async Task<IDeferredQuickInfoContent> BuildContentAsync(
            Document document,
            ISyntaxToken token,
            CancellationToken cancellationToken)
        {
            var modelAndSymbols = await this.BindTokenAsync(document, token, cancellationToken).ConfigureAwait(false);
            if (modelAndSymbols.Item2.Length == 0)
            {
                return null;
            }

            return await CreateContentAsync(document.Workspace,
                token,
                modelAndSymbols.Item1,
                modelAndSymbols.Item2,
                cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        protected async Task<IDeferredQuickInfoContent> CreateContentAsync(
            Workspace workspace,
            ISyntaxToken token,
            SemanticModelBase semanticModel,
            IEnumerable<ISymbol> symbols,
            CancellationToken cancellationToken)
        {
            var descriptionService = workspace.Services.GetLanguageServices(token.Language).GetService<ISymbolDisplayService>();

            var sections = await descriptionService.ToDescriptionGroupsAsync(workspace, semanticModel, token.FileSpan.Span.Start, symbols.ToImmutableArray(), cancellationToken).ConfigureAwait(false);

            var mainDescriptionBuilder = new List<TaggedText>();
            if (sections.ContainsKey(SymbolDescriptionGroups.MainDescription))
            {
                mainDescriptionBuilder.AddRange(sections[SymbolDescriptionGroups.MainDescription]);
            }

            var documentationContent = GetDocumentationContent(symbols, sections, token, cancellationToken);
            var showSymbolGlyph = true;

            return this.CreateQuickInfoDisplayDeferredContent(
                symbol: symbols.First(),
                showWarningGlyph: false,
                showSymbolGlyph: showSymbolGlyph,
                mainDescription: mainDescriptionBuilder,
                documentation: documentationContent,
                typeParameterMap: SpecializedCollections.EmptyList<TaggedText>(),
                anonymousTypes: SpecializedCollections.EmptyList<TaggedText>(),
                usageText: SpecializedCollections.EmptyList<TaggedText>(),
                exceptionText: SpecializedCollections.EmptyList<TaggedText>());
        }

        private IDeferredQuickInfoContent GetDocumentationContent(
            IEnumerable<ISymbol> symbols,
            IDictionary<SymbolDescriptionGroups, ImmutableArray<TaggedText>> sections,
            ISyntaxToken token,
            CancellationToken cancellationToken)
        {
            if (sections.ContainsKey(SymbolDescriptionGroups.Documentation))
            {
                var documentationBuilder = new List<TaggedText>();
                documentationBuilder.AddRange(sections[SymbolDescriptionGroups.Documentation]);
                return CreateClassifiableDeferredContent(documentationBuilder);
            }
            else if (symbols.Any())
            {
                var documentationBuilder = new List<TaggedText>();
                var firstSymbol = symbols.First();
                if (!string.IsNullOrEmpty(firstSymbol.Documentation))
                    documentationBuilder.AddText(firstSymbol.Documentation);
                return CreateClassifiableDeferredContent(documentationBuilder);
            }

            return CreateDocumentationCommentDeferredContent(null);
        }

        private async Task<ValueTuple<SemanticModelBase, ImmutableArray<ISymbol>>> BindTokenAsync(
            Document document,
            ISyntaxToken token,
            CancellationToken cancellationToken)
        {
            var syntaxTree = await document.GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(false);
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            var symbols = semanticModel.GetSemanticInfo(token, document.Workspace, cancellationToken)
                                       .GetSymbols(includeType: true);

            symbols = symbols.Distinct().ToImmutableArray();

            if (symbols.Any())
            {
                return ValueTuple.Create(semanticModel, symbols);
            }

            return ValueTuple.Create(semanticModel, ImmutableArray<ISymbol>.Empty);
        }
    }
}
