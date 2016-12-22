using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace ShaderTools.Editor.VisualStudio.Core.Util.Extensions
{
    internal static class IVsShellExtensions
    {
        public static T LoadPackage<T>(this IVsShell shell)
           where T : Package
        {
            var guid = typeof(T).GUID;
            IVsPackage package;
            ErrorHandler.ThrowOnFailure(shell.LoadPackage(ref guid, out package));
            return (T) package;
        }
    }
}