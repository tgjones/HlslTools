using System.Drawing;
using ShaderTools.CodeAnalysis.NavigateTo;

namespace ShaderTools.CodeAnalysis.Editor.Implementation.NavigateTo
{
    internal sealed class NavigateToItemDisplay : AbstractNavigateToItemDisplay
    {
        private readonly NavigateToIconFactory _iconFactory;

        private Icon _glyph;

        public NavigateToItemDisplay(INavigateToSearchResult searchResult, NavigateToIconFactory iconFactory)
            : base(searchResult)
        {
            _iconFactory = iconFactory;
        }

        public override Icon Glyph
        {
            get
            {
                if (_glyph == null)
                {
                    _glyph = _iconFactory.GetIcon(SearchResult.NavigableItem.Glyph);
                }

                return _glyph;
            }
        }
    }
}
