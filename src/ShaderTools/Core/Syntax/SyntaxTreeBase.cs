using ShaderTools.Core.Text;

namespace ShaderTools.Core.Syntax
{
    public abstract class SyntaxTreeBase
    {
        public abstract TextSpan GetSourceTextSpan(SourceRange range);
    }
}
