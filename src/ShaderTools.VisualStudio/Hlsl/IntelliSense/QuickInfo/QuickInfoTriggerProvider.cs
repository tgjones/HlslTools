using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace ShaderTools.VisualStudio.Hlsl.IntelliSense.QuickInfo
{
    [Export(typeof(IWpfTextViewCreationListener))]
    [Name("QuickInfoTriggerProvider")]
    [ContentType(HlslConstants.ContentTypeName)]
    [TextViewRole(PredefinedTextViewRoles.Interactive)]
    internal sealed class QuickInfoTriggerProvider : IWpfTextViewCreationListener
    {
        [Import]
        public QuickInfoManagerProvider QuickInfoManagerProvider { get; set; }

        public void TextViewCreated(IWpfTextView textView)
        {
            var quickInfoManager = QuickInfoManagerProvider.GetQuickInfoManager(textView);
            new QuickInfoTrigger(textView, quickInfoManager);
        }
    }
}