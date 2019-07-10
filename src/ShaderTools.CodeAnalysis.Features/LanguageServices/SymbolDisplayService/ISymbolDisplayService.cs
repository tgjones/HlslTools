// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Host;
using ShaderTools.CodeAnalysis.Compilation;
using ShaderTools.CodeAnalysis.Symbols;
using TaggedText = Microsoft.CodeAnalysis.TaggedText;

namespace ShaderTools.CodeAnalysis.LanguageServices
{
    internal interface ISymbolDisplayService : ILanguageService
    {
        Task<IDictionary<SymbolDescriptionGroups, ImmutableArray<TaggedText>>> ToDescriptionGroupsAsync(Workspace workspace, SemanticModelBase semanticModel, int position, ImmutableArray<ISymbol> symbols, CancellationToken cancellationToken = default(CancellationToken));
    }
}