using System.ComponentModel.Composition.Hosting;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace ShaderTools.Editor.VisualStudio.Tests.Hlsl.Support
{
    internal static class TextViewUtility
    {
        public static ITextView CreateTextView(CompositionContainer container, ITextBuffer buffer)
        {
            var editorAdaptersFactory = container.GetExportedValue<ITextEditorFactoryService>();
            return editorAdaptersFactory.CreateTextView(buffer);
        }
    }
}