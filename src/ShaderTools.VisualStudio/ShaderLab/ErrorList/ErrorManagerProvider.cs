using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using ShaderTools.VisualStudio.ShaderLab.Options;
using ShaderTools.VisualStudio.ShaderLab.Util.Extensions;

namespace ShaderTools.VisualStudio.ShaderLab.ErrorList
{
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType(ShaderLabConstants.ContentTypeName)]
    [TextViewRole(PredefinedTextViewRoles.Interactive)]
    internal sealed class ErrorManagerProvider : IWpfTextViewCreationListener
    {
        [Import]
        public IShaderLabOptionsService OptionsService { get; set; }

        [Import]
        public SVsServiceProvider ServiceProvider { get; set; }

        [Import]
        public ITextDocumentFactoryService TextDocumentFactoryService { get; set; }

        public void TextViewCreated(IWpfTextView textView)
        {
            textView.Properties.GetOrCreateSingletonProperty(() => new SyntaxErrorManager(textView.TextBuffer.GetBackgroundParser(), textView, OptionsService, ServiceProvider, TextDocumentFactoryService));
        }
    }
}