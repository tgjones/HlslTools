using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;
using ShaderTools.CodeAnalysis.Editor.Implementation.IntelliSense;
using ShaderTools.CodeAnalysis.Editor.Shared.Utilities;
using ShaderTools.CodeAnalysis.QuickInfo;

namespace ShaderTools.CodeAnalysis.Editor.Implementation.QuickInfo
{
    [Export(typeof(IAsyncQuickInfoSourceProvider))]
    [ContentType(ContentTypeNames.ShaderToolsContentType)]
    [Name(nameof(QuickInfoSourceProvider))]
    internal sealed class QuickInfoSourceProvider : ForegroundThreadAffinitizedObject, IAsyncQuickInfoSourceProvider
    {
        private readonly QuickInfoService _quickInfoService;

        public string DisplayName => "Quick Info";

        [ImportingConstructor]
        public QuickInfoSourceProvider(QuickInfoService quickInfoService)
        {
            _quickInfoService = quickInfoService;
        }

        public IAsyncQuickInfoSource TryCreateQuickInfoSource(ITextBuffer textBuffer)
        {
            return textBuffer.Properties.GetOrCreateSingletonProperty(() => new QuickInfoSource(textBuffer, new DocumentProvider(), _quickInfoService));
        }
    }
}
