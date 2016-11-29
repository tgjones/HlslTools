using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using ShaderTools.VisualStudio.Hlsl.Glyphs;
using ShaderTools.VisualStudio.Hlsl.Util.Extensions;

namespace ShaderTools.VisualStudio.Hlsl.Navigation
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