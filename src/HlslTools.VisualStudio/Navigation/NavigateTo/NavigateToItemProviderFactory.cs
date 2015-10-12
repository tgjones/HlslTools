using System;
using System.ComponentModel.Composition;
using HlslTools.VisualStudio.Glyphs;
using HlslTools.VisualStudio.Text;
using Microsoft.VisualStudio.Language.NavigateTo.Interfaces;
using Microsoft.VisualStudio.Text.Projection;

namespace HlslTools.VisualStudio.Navigation.NavigateTo
{
    [Export(typeof(INavigateToItemProviderFactory))]
    internal sealed class NavigateToItemProviderFactory : INavigateToItemProviderFactory, INavigateToItemDisplayFactory
    {
        [Import]
        public IBufferGraphFactoryService BufferGraphFactoryService { get; set; }

        [Import]
        public DispatcherGlyphService DispatcherGlyphService { get; set; }

        [Import]
        public VisualStudioSourceTextFactory SourceTextFactory { get; set; }

        public bool TryCreateNavigateToItemProvider(IServiceProvider serviceProvider, out INavigateToItemProvider provider)
        {
            provider = new NavigateToItemProvider(BufferGraphFactoryService, this, DispatcherGlyphService, serviceProvider, SourceTextFactory);
            return true;
        }

        public INavigateToItemDisplay CreateItemDisplay(NavigateToItem item)
        {
            return item.Tag as INavigateToItemDisplay;
        }
    }
}