using Microsoft.VisualStudio.Text;
using ShaderTools.Editor.VisualStudio.Core.Parsing;

namespace ShaderTools.Editor.VisualStudio.ShaderLab.Parsing
{
    internal sealed class BackgroundParser : BackgroundParserBase
    {
        public BackgroundParser(ITextBuffer textBuffer)
            : base(textBuffer)
        {
        }
    }
}
