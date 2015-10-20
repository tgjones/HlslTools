using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;

namespace HlslTools.VisualStudio.IntelliSense.SignatureHelp
{
    [Export(typeof(ISignatureHelpSourceProvider))]
    [ContentType(HlslConstants.ContentTypeName)]
    [Name(nameof(SignatureHelpSourceProvider))]
    internal sealed class SignatureHelpSourceProvider : ISignatureHelpSourceProvider
    {
        public ISignatureHelpSource TryCreateSignatureHelpSource(ITextBuffer textBuffer)
        {
            return textBuffer.Properties.GetOrCreateSingletonProperty(() => new SignatureHelpSource(textBuffer));
        }
    }
}