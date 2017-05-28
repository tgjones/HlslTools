using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;

namespace ShaderTools.Editor.VisualStudio.Core.Util.Extensions
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
    }
}