using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using ShaderTools.Editor.VisualStudio.Core.Glyphs;

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
                () => new EditorNavigationSource(textBuffer, GlyphService));
        }
    }
}