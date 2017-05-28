// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using ShaderTools.CodeAnalysis.Editor.Host;
using ShaderTools.CodeAnalysis.FindUsages;
using ShaderTools.Utilities.Threading;

namespace ShaderTools.CodeAnalysis.Editor.GoToDefinition
{
    internal static class GoToDefinitionHelpers
    {
        public static bool TryGoToDefinition(
            ImmutableArray<DefinitionItem> definitions,
            IEnumerable<Lazy<IStreamingFindUsagesPresenter>> streamingPresenters,
            CancellationToken cancellationToken)
        {
            var presenter = GetFindUsagesPresenter(streamingPresenters);
            var title = string.Empty; // string.Format("'{0}' declarations", definitions.Name);

            return presenter.TryNavigateToOrPresentItemsAsync(
                title, definitions).WaitAndGetResult(cancellationToken);
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