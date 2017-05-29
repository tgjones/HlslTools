using System;
using Microsoft.VisualStudio.Language.NavigateTo.Interfaces;
using ShaderTools.CodeAnalysis.NavigateTo;
using ShaderTools.Utilities.Diagnostics;

namespace ShaderTools.CodeAnalysis.Editor.Implementation.NavigateTo
{
    internal sealed class NavigateToItemDisplayFactory : INavigateToItemDisplayFactory, IDisposable
    {
        private readonly NavigateToIconFactory _iconFactory;

        public NavigateToItemDisplayFactory(NavigateToIconFactory iconFactory)
        {
            Contract.ThrowIfNull(iconFactory);

            _iconFactory = iconFactory;
        }

        public INavigateToItemDisplay CreateItemDisplay(NavigateToItem item)
        {
            var searchResult = (INavigateToSearchResult) item.Tag;
            return new NavigateToItemDisplay(searchResult, _iconFactory);
        }

        public void Dispose()
        {
            _iconFactory.Dispose();
        }
    }
}
