using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;
using ShaderTools.CodeAnalysis.Editor.Implementation.IntelliSense;
using ShaderTools.CodeAnalysis.QuickInfo;

namespace ShaderTools.CodeAnalysis.Editor.Implementation.QuickInfo
{
    [Export(typeof(IAsyncQuickInfoSourceProvider))]
    [ContentType(ContentTypeNames.ShaderToolsContentType)]
    [Name(nameof(QuickInfoSourceProvider))]
    internal sealed class QuickInfoSourceProvider : IAsyncQuickInfoSourceProvider
    {
        public string DisplayName => "Quick Info";

        public IAsyncQuickInfoSource TryCreateQuickInfoSource(ITextBuffer textBuffer)
        {
            return textBuffer.Properties.GetOrCreateSingletonProperty(() => new QuickInfoSource(textBuffer, new DocumentProvider()));
        }
    }
}
