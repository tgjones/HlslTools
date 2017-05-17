using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;

namespace ShaderTools.VisualStudio.SyntaxVisualizer.Utilities
{
    internal static class IVsWindowFrameExtensions
    {
        public static IWpfTextView GetWpfTextView(this IVsWindowFrame vsWindowFrame)
        {
            IWpfTextView wpfTextView = null;
            var textView = VsShellUtilities.GetTextView(vsWindowFrame);
            if (textView != null)
            {
                var riidKey = DefGuidList.guidIWpfTextViewHost;
                object pvtData;
                if (((IVsUserData) textView).GetData(ref riidKey, out pvtData) == 0 && pvtData != null)
                    wpfTextView = ((IWpfTextViewHost)pvtData).TextView;
            }
            return wpfTextView;
        }
    }
}
