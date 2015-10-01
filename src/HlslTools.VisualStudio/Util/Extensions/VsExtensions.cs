using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;

namespace HlslTools.VisualStudio.Util.Extensions
{
    internal static class VsExtensions
    {
        public static IVsTextView GetPrimaryView(this IVsCodeWindow codeWindow)
        {
            IVsTextView view;
            if (ErrorHandler.Failed(codeWindow.GetPrimaryView(out view)))
                return null;

            return view;
        }

        public static IVsTextView GetSecondaryView(this IVsCodeWindow codeWindow)
        {
            IVsTextView view;
            if (ErrorHandler.Failed(codeWindow.GetSecondaryView(out view)))
                return null;

            return view;
        }

        public static IVsTextView GetLastActiveView(this IVsCodeWindow codeWindow)
        {
            IVsTextView view;
            if (ErrorHandler.Failed(codeWindow.GetLastActiveView(out view)))
                return null;

            return view;
        }

        public static IWpfTextView GetWpfTextView(this IVsWindowFrame vsWindowFrame)
        {
            IWpfTextView wpfTextView = null;
            var textView = VsShellUtilities.GetTextView(vsWindowFrame);
            if (textView != null)
            {
                var riidKey = DefGuidList.guidIWpfTextViewHost;
                object pvtData;
                if (((IVsUserData)textView).GetData(ref riidKey, out pvtData) == 0 && pvtData != null)
                    wpfTextView = ((IWpfTextViewHost)pvtData).TextView;
            }
            return wpfTextView;
        }

        public static ITextDocument GetTextDocument(this ITextBuffer textBuffer)
        {
            ITextDocument textDoc;
            var rc = textBuffer.Properties.TryGetProperty(typeof(ITextDocument), out textDoc);
            return (rc) ? textDoc : null;
        }
    }
}