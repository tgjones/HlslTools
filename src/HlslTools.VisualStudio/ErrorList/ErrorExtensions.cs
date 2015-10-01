using Microsoft.VisualStudio.Text;

namespace HlslTools.VisualStudio.ErrorList
{
    internal static class ErrorExtensions
    {
        public static ErrorListHelper GetErrorListHelper(this ITextBuffer textBuffer)
        {
            ErrorListHelper result;
            textBuffer.Properties.TryGetProperty(typeof(ErrorListHelper), out result);
            return result;
        }
    }
}