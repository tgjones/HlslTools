// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ShaderTools.CodeAnalysis.Editor.Host;
using ShaderTools.CodeAnalysis.FindSymbols;
using ShaderTools.CodeAnalysis.FindUsages;
using ShaderTools.CodeAnalysis.Symbols;
using ShaderTools.Utilities.PooledObjects;
using ShaderTools.Utilities.Threading;

namespace ShaderTools.CodeAnalysis.Editor.GoToDefinition
{
    internal static class GoToDefinitionHelpers
    {
        public static bool TryGoToDefinition(
            ISymbol symbol,
            Workspace workspace,
            IEnumerable<Lazy<IStreamingFindUsagesPresenter>> streamingPresenters,
            CancellationToken cancellationToken)
        {
            var definition = SymbolFinder.FindSourceDefinitionAsync(symbol, cancellationToken).WaitAndGetResult(cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            symbol = definition ?? symbol;

            if (symbol.Locations.IsEmpty)
            {
                return false;
            }

            var definitions = ArrayBuilder<DefinitionItem>.GetInstance();

            var options = workspace.Options;

            definitions.Add(symbol.ToDefinitionItem(workspace));

            var presenter = GetFindUsagesPresenter(streamingPresenters);
            var title = string.Format("'{0}' declarations",
                symbol.Name);

            return presenter.TryNavigateToOrPresentItemsAsync(
                title, definitions.ToImmutableAndFree()).WaitAndGetResult(cancellationToken);
        }

        private static IStreamingFindUsagesPresenter GetFindUsagesPresenter(
            IEnumerable<Lazy<IStreamingFindUsagesPresenter>> streamingPresenters)
        {
            try
            {
                return streamingPresenters.FirstOrDefault()?.Value;
            }
            catch
            {
                return null;
            }
        }
    }
}