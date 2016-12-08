using Microsoft.VisualStudio.Text;

namespace ShaderTools.VisualStudio.Core.Util.Extensions
{
    /// <summary>
    /// Extension methods for the editor Span struct
    /// </summary>
    internal static class SpanExtensions
    {
        public static bool IntersectsWith(this Span span, int position)
        {
            return position >= span.Start && position <= span.End;
        }
    }
}