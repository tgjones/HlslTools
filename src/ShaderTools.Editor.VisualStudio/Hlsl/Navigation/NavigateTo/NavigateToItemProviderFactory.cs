using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.NavigateTo.Interfaces;
using Microsoft.VisualStudio.Text.Projection;
using ShaderTools.Editor.VisualStudio.Core.Glyphs;

namespace ShaderTools.Editor.VisualStudio.Hlsl.Navigation.NavigateTo
{
    [Export(typeof(INavigateToItemProviderFactory))]
    internal sealed class NavigateToItemProviderFactory : INavigateToItemProviderFactory, INavigateToItemDisplayFactory
    {
        [Import]
        public IBufferGraphFactoryService BufferGraphFactoryService { get; set; }

        [Import]
        public DispatcherGlyphService DispatcherGlyphService { get; set; }

        public bool TryCreateNavigateToItemProvider(IServiceProvider serviceProvider, out INavigateToItemProvider provider)
        {
            provider = new NavigateToItemProvider(BufferGraphFactoryService, this, DispatcherGlyphService, serviceProvider);
            return true;
        }

        public INavigateToItemDisplay CreateItemDisplay(NavigateToItem item)
        {
            return item.Tag as INavigateToItemDisplay;
        }
    }
}