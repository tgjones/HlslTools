using Microsoft.VisualStudio.Text;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.Editor.VisualStudio.Core.Util.Extensions
{
    internal static class TextSpanExtensions
    {
        public static Span ToSpan(this TextSpan textSpan)
        {
            return new Span(textSpan.Start, textSpan.Length);
        }
    }
}
