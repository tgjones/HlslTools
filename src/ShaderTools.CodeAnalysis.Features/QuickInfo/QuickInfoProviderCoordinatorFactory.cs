using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using ShaderTools.CodeAnalysis.Host.Mef;
using ShaderTools.CodeAnalysis.Shared.Utilities;

namespace ShaderTools.CodeAnalysis.QuickInfo
{
    [Export(typeof(IQuickInfoProviderCoordinatorFactory))]
    internal sealed class QuickInfoProviderCoordinatorFactory : IQuickInfoProviderCoordinatorFactory
    {
        private readonly IEnumerable<Lazy<IQuickInfoProvider, OrderableLanguageMetadata>> _providers;

        [ImportingConstructor]
        public QuickInfoProviderCoordinatorFactory(
            [ImportMany] IEnumerable<Lazy<IQuickInfoProvider, OrderableLanguageMetadata>> providers)
        {
            _providers = ExtensionOrderer.Order(providers);
        }

        public IQuickInfoProviderCoordinator CreateCoordinator(Document document)
        {
            return new QuickInfoProviderCoordinator(_providers
                .Where(x => x.Metadata.Language == document.Language)
                .Select(x => x.Value)
                .ToList());
        }
    }
}