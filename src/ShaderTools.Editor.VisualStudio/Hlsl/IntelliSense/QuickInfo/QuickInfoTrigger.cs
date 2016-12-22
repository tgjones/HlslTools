using Microsoft.VisualStudio.Text.Editor;

namespace ShaderTools.Editor.VisualStudio.Hlsl.IntelliSense.QuickInfo
{
    internal sealed class QuickInfoTrigger
    {
        private readonly IWpfTextView _wpfTextView;
        private readonly QuickInfoManager _quickInfoManager;

        public QuickInfoTrigger(IWpfTextView wpfTextView, QuickInfoManager quickInfoManager)
        {
            _wpfTextView = wpfTextView;
            _wpfTextView.MouseHover += WpfTextViewOnMouseHover;
            _quickInfoManager = quickInfoManager;
        }

        private void WpfTextViewOnMouseHover(object sender, MouseHoverEventArgs e)
        {
            _quickInfoManager.TriggerQuickInfo(e.Position);
        }
    }
}