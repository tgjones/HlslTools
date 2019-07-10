// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ShaderTools.CodeAnalysis.Compilation;
using ShaderTools.CodeAnalysis.LanguageServices;
using ShaderTools.CodeAnalysis.Shared.Extensions;
using ShaderTools.CodeAnalysis.Symbols;
using ShaderTools.CodeAnalysis.Syntax;
using ShaderTools.Utilities.Collections;
using TaggedText = Microsoft.CodeAnalysis.TaggedText;

namespace ShaderTools.CodeAnalysis.QuickInfo
{
    internal abstract class AbstractSemanticQuickInfoProvider : AbstractQuickInfoProvider
    {
        protected override async Task<QuickInfoContent> BuildContentAsync(
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

        protected async Task<QuickInfoContent> CreateContentAsync(
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

            var documentationContent = GetDocumentationContent(symbols, sections);

            return new QuickInfoContent(
                semanticModel.Language,
                glyph: symbols?.First().GetGlyph() ?? Glyph.None,
                mainDescription: ImmutableArray.CreateRange(mainDescriptionBuilder),
                documentation: documentationContent);
        }

        private ImmutableArray<TaggedText> GetDocumentationContent(
            IEnumerable<ISymbol> symbols,
            IDictionary<SymbolDescriptionGroups, ImmutableArray<TaggedText>> sections)
        {
            if (sections.ContainsKey(SymbolDescriptionGroups.Documentation))
            {
                return sections[SymbolDescriptionGroups.Documentation];
            }

            if (symbols.Any())
            {
                var documentationBuilder = new List<TaggedText>();
                var firstSymbol = symbols.First();
                if (!string.IsNullOrEmpty(firstSymbol.Documentation))
                    documentationBuilder.AddText(firstSymbol.Documentation);
                return ImmutableArray.CreateRange(documentationBuilder);
            }

            return ImmutableArray<TaggedText>.Empty;
        }

        private async Task<ValueTuple<SemanticModelBase, ImmutableArray<ISymbol>>> BindTokenAsync(
            Document document,
            ISyntaxToken token,
            CancellationToken cancellationToken)
        {
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

            if (semanticModel != null)
            {
                var symbols = semanticModel.GetSemanticInfo(token, document.Workspace, cancellationToken)
                                           .GetSymbols(includeType: true);

                symbols = symbols.Distinct().ToImmutableArray();

                if (symbols.Any())
                {
                    return ValueTuple.Create(semanticModel, symbols);
                }
            }

            return ValueTuple.Create(semanticModel, ImmutableArray<ISymbol>.Empty);
        }
    }
}
