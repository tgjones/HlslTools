using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using ShaderTools.Editor.VisualStudio.Core.Glyphs;
using ShaderTools.Editor.VisualStudio.Hlsl.Util.Extensions;

namespace ShaderTools.Editor.VisualStudio.Hlsl.Navigation
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