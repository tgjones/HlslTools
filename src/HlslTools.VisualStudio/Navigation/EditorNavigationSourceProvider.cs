using System.ComponentModel.Composition;
using HlslTools.VisualStudio.Glyphs;
using HlslTools.VisualStudio.Util.Extensions;
using Microsoft.VisualStudio.Text;

namespace HlslTools.VisualStudio.Navigation
{
    [Export]
    internal class EditorNavigationSourceProvider
    {
        [Import]
        public DispatcherGlyphService GlyphService { get; private set; }

        public EditorNavigationSource TryCreateEditorNavigationSource(ITextBuffer textBuffer)
        {
            return textBuffer.Properties.GetOrCreateSingletonProperty(
                () =>
                {
                    var result = new EditorNavigationSource(textBuffer, textBuffer.GetBackgroundParser(), GlyphService);
                    result.Initialize();
                    return result;
                });
        }
    }
}