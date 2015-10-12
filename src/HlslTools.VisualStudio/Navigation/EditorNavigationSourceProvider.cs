using System.ComponentModel.Composition;
using HlslTools.VisualStudio.Glyphs;
using HlslTools.VisualStudio.Text;
using HlslTools.VisualStudio.Util.Extensions;
using Microsoft.VisualStudio.Text;

namespace HlslTools.VisualStudio.Navigation
{
    [Export]
    internal class EditorNavigationSourceProvider
    {
        [Import]
        public DispatcherGlyphService GlyphService { get; private set; }

        [Import]
        public VisualStudioSourceTextFactory SourceTextFactory { get; set; }

        public EditorNavigationSource TryCreateEditorNavigationSource(ITextBuffer textBuffer)
        {
            return textBuffer.Properties.GetOrCreateSingletonProperty(
                () =>
                {
                    var result = new EditorNavigationSource(textBuffer, textBuffer.GetBackgroundParser(SourceTextFactory), GlyphService, SourceTextFactory);
                    result.Initialize();
                    return result;
                });
        }
    }
}